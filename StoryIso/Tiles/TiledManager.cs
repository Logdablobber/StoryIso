using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using AsepriteDotNet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Collisions.Layers;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Renderers;
using StoryIso.Enums;
using StoryIso.Functions;
using StoryIso.Misc;

namespace StoryIso.Tiled;

public class TiledManager
{
	private Dictionary<string, TilemapRoom>? _rooms;

	public string? currentRoomName;
	public TilemapRoom currentRoom
	{
		get
		{
			return _rooms![currentRoomName!];
		}
	}
	public int TilesetCount
	{
		get
		{
			return currentRoom.map.Tilesets[0].TileCount;
		}
	}

	private string? map_to_load = null;
	private bool refresh_map = false;

	private readonly TiledMapRenderer _tiledMapRenderer;

	private readonly System.Threading.Lock _mapLoadLock = new();
	private readonly System.Threading.Lock _refreshMapLock = new();

	public TiledManager(GraphicsDevice graphics, ContentManager content, string start_room)
	{
		_tiledMapRenderer = new TiledMapRenderer(graphics);
		LoadMaps(content);
		LoadMap(start_room);
	}

	private void LoadMaps(ContentManager content)
	{
		_rooms = new Dictionary<string, TilemapRoom>();

		string[] files = Directory.GetFiles("Content/Tiled/Maps/", "*.xnb");

		foreach (string path in files)
		{
			FileInfo file = new(path);
			string file_name = file.Name[..^4]; // strips ending .xnb

			TiledMap map = content.Load<TiledMap>("Tiled/Maps/" + file_name);

			_rooms.Add(map.Type, GenerateMap(map));
		}
	}

	public void Update(GameTime gameTime)
	{
		CheckLoadMap();
		CheckRefreshMap();
		_tiledMapRenderer.Update(gameTime);
	}

	public void LoadMapThread(string map_name)
	{
		lock (_mapLoadLock)
		{
			map_to_load = map_name;
		}
	}

	public void LoadMap(string map_name)
	{
		if (_rooms == null)
		{
			throw new NullReferenceException("Rooms is null");
		}

		if (_rooms.TryGetValue(map_name, out TilemapRoom? room))
		{
			currentRoomName = map_name;
			_tiledMapRenderer.LoadMap(room.map);
		}
	}

	private void CheckLoadMap()
	{
		string new_map;
		lock (_mapLoadLock)
		{
			if (map_to_load == null)
			{
				return;
			}

			new_map = map_to_load;
			map_to_load = null;
		}

		LoadMap(new_map);
	}

	public void RefreshMap()
	{
		_tiledMapRenderer.LoadMap(currentRoom.map);
	}

	public void RefreshMapThread()
	{
		lock (_refreshMapLock)
		{
			refresh_map = true;
		}
	}

	private void CheckRefreshMap()
	{
		lock (_refreshMapLock)
		{
			if (!refresh_map)
			{
				return;
			}

			refresh_map = false;
		}

		RefreshMap();
	}

	private TilemapRoom GenerateMap(TiledMap map)
	{
		Dictionary<string, Collider> collision_rectangles = [];

		LayerIndices layer_indices = new();

		for (int i = 0; i < map.TileLayers.Count; i++)
		{
			if (map.TileLayers[i].Name.ToLower() == "floor layer")
			{
				layer_indices.floorTileLayerIndex = i;
				continue;
			}

			if (map.TileLayers[i].Name.ToLower() == "wall layer")
			{
				layer_indices.wallTileLayerIndex = i;
				continue;
			}

			if (map.TileLayers[i].Name.ToLower() == "interaction tile layer")
			{
				layer_indices.interactionTileLayerIndex = i;
			}

			if (layer_indices.floorTileLayerIndex.HasValue &&
				layer_indices.wallTileLayerIndex.HasValue &&
				layer_indices.interactionLayerIndex.HasValue) 
			{
				break;
			}
		}

		for (int i = 0; i < map.ObjectLayers.Count; i++)
		{
			if (map.ObjectLayers[i].Name.ToLower() == "collision layer")
			{
				layer_indices.collisionLayerIndex = i;
				continue;
			}

			if (map.ObjectLayers[i].Name.ToLower() == "trigger layer")
			{
				layer_indices.triggerLayerIndex = i;
				continue;
			}

			if (map.ObjectLayers[i].Name.ToLower() == "interaction layer")
			{
				layer_indices.interactionLayerIndex = i;
			}

			if (layer_indices.collisionLayerIndex.HasValue && 
				layer_indices.triggerLayerIndex.HasValue &&
				layer_indices.interactionLayerIndex.HasValue)
			{
				break;
			}
		}

		if (layer_indices.collisionLayerIndex.HasValue)
		{
			foreach (var obj in map.ObjectLayers[layer_indices.collisionLayerIndex.Value].Objects)
			{
				bool enabled = true;
				string name = obj.Name != "" ? obj.Name : obj.Identifier.ToString();

				if (obj.Properties.TryGetValue("enabled", out string enabled_value))
				{
					if (enabled_value == "false")
					{
						enabled = false;
					}
				}

				collision_rectangles.Add(name, new Collider(new RectangleF(obj.Position, obj.Size), enabled));
			}
		}

		List<Trigger> triggers = [];

		if (layer_indices.triggerLayerIndex.HasValue)
		{
			foreach (var obj in map.ObjectLayers[layer_indices.triggerLayerIndex.Value].Objects)
			{
				List<Function>? on_enter = [];
				List<Function>? on_exit = [];
				List<Function>? on_stay = [];
				Color color = Color.White;
				
				if (obj.Properties.TryGetValue("onEnter", out string enter_code))
				{
					on_enter = FunctionProcessor.Process($"Trigger {obj.Identifier}", enter_code);
				}

				if (obj.Properties.TryGetValue("onExit", out string exit_code))
				{
					on_exit = FunctionProcessor.Process($"Trigger {obj.Identifier}", exit_code);
				}

				if (obj.Properties.TryGetValue("onStay", out string stay_code))
				{
					on_stay = FunctionProcessor.Process($"Trigger {obj.Identifier}", stay_code);
				}

				if (obj.Properties.TryGetValue("color", out string c))
				{
					color = ColorHelper.FromHex(c[0] + c[3..] + c[1..3]); // alpha needs to be the the end
				}

				triggers.Add(new Trigger(obj.Identifier, new Rectangle(obj.Position.ToPoint(), (Point)obj.Size), on_enter, on_exit, on_stay, color));
			}
		}

		List<InteractionTile> interaction_tiles = [];

		if (layer_indices.interactionLayerIndex.HasValue)
		{
			foreach (var obj in map.ObjectLayers[layer_indices.interactionLayerIndex.Value].Objects)
			{
				List<Function>? on_interact = [];
				List<Function>? on_uninteract = [];
				List<Function>? while_interact = [];
				List<Function>? on_toggle_on = [];
				List<Function>? on_toggle_off = [];
				bool default_toggle_state = false;
				bool collides = true;
				
				if (obj.Properties.TryGetValue("onInteract", out string enter_code))
				{
					on_interact = FunctionProcessor.Process($"Interaction {obj.Identifier}", enter_code);
				}
				if (obj.Properties.TryGetValue("onUninteract", out string exit_code))
				{
					on_uninteract = FunctionProcessor.Process($"Interaction {obj.Identifier}", exit_code);
				}
				if (obj.Properties.TryGetValue("whileInteract", out string stay_code))
				{
					while_interact = FunctionProcessor.Process($"Interaction {obj.Identifier}", stay_code);
				}
				if (obj.Properties.TryGetValue("onToggleOn", out string on_code))
				{
					on_toggle_on = FunctionProcessor.Process($"Interaction {obj.Identifier}", on_code);
				}
				if (obj.Properties.TryGetValue("onToggleOff", out string off_code))
				{
					on_toggle_off = FunctionProcessor.Process($"Interaction {obj.Identifier}", off_code);
				}
				if (obj.Properties.TryGetValue("defaultToggleState", out string state))
				{
					if (state == "true")
					{
						default_toggle_state = true;
					}
				}
				if (obj.Properties.TryGetValue("collides", out string collide))
				{
					if (collide == "false")
					{
						collides = false;
					}
				}

				interaction_tiles.Add(new InteractionTile(obj.Identifier,
														new Rectangle(obj.Position.ToPoint(), (Point)obj.Size), 
														on_interact: on_interact, 
														on_uninteract: on_uninteract, 
														while_interact: while_interact,
														on_toggle_on: on_toggle_on,
														on_toggle_off: on_toggle_off,
														default_toggle_state: default_toggle_state));

				if (collides)
				{
					string name = obj.Name != "" ? obj.Name : obj.Identifier.ToString();
					collision_rectangles.Add(name, new Collider(new RectangleF(obj.Position, obj.Size), true));
				}
			}
		}

		return new TilemapRoom(map, layer_indices, collision_rectangles, interaction_tiles, triggers);
	}

	public Vector2 TilePosToWorldPos(Point tile)
	{
		return TilePosToWorldPos(tile.X, tile.Y);
	}

	public Vector2 TilePosToWorldPos(int x, int y)
	{
		return new Vector2(x * currentRoom.map.TileWidth, y * currentRoom.map.TileHeight);
	}

	public RelativeVector2 TilePosToWorldPos(RelativeVariable<int> x, RelativeVariable<int> y)
	{
		return new RelativeVector2(new RelativeVariable<float>(x.Value * currentRoom.map.TileWidth, x.Relative), new RelativeVariable<float>(y.Value * currentRoom.map.TileHeight, y.Relative));
	}

	public RelativeVector2 TilePosToWorldPos(RelativeVariable<float> x, RelativeVariable<float> y)
	{
		return new RelativeVector2(new RelativeVariable<float>(x.Value * currentRoom.map.TileWidth, x.Relative), new RelativeVariable<float>(y.Value * currentRoom.map.TileHeight, y.Relative));
	}

	public RelativeVector2 TilePosToWorldPos(RelativeVariable<FunctionParameter<int>> x, RelativeVariable<FunctionParameter<int>> y)
	{
		Optional<int> x_value = x.Value.Value;
		Optional<int> y_value = y.Value.Value;

		if (!x_value.HasValue || !y_value.HasValue)
		{
			return new RelativeVector2(new RelativeVariable<float>(0, x.Relative), new RelativeVariable<float>(0, y.Relative));
		}

		return new RelativeVector2(new RelativeVariable<float>(x_value.Value * currentRoom.map.TileWidth, x.Relative), new RelativeVariable<float>(y_value.Value * currentRoom.map.TileHeight, y.Relative));
	}

	public Point WorldPosToTilePos(Point point)
	{
		return point / new Point(currentRoom.map.TileWidth, currentRoom.map.TileHeight);
	}

	public Vector2 WorldPosToTilePos(Vector2 v2)
	{
		return v2 / new Vector2(currentRoom.map.TileWidth, currentRoom.map.TileHeight);
	}

	public float WorldXToTileX(float x)
	{
		return x / currentRoom.map.TileWidth;
	}

	public float WorldYToTileY(float y)
	{
		return y / currentRoom.map.TileHeight;
	}

	public float TileXToWorldX(float x)
	{
		return x * currentRoom.map.TileWidth;
	}

	public float TileYToWorldY(float y)
	{
		return y * currentRoom.map.TileHeight;
	}

	public void Draw()
	{
		_tiledMapRenderer.Draw(Game1.camera.GetViewMatrix());
	}
}