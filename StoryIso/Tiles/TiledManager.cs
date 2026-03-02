using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotTiled;
using DotTiled.Serialization;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Content;
using StoryIso.Debugging;
using StoryIso.FileLoading;
using StoryIso.Functions;
using StoryIso.Misc;

namespace StoryIso.Tiled;

public class TiledManager
{
	private Dictionary<string, TilemapRoom>? _rooms;

	public string? currentRoomName;
	public TilemapRoom? currentRoom
	{
		get
		{
			if (currentRoomName == null || _rooms == null)
			{
				return null;
			}

			return _rooms[currentRoomName];
		}
	}
	public int? TilesetCount
	{
		get
		{
			if (currentRoom == null)
			{
				return null;
			}

			return currentRoom.map.Tilesets[0].TileCount;
		}
	}

	private string? map_to_load = null;

	private readonly TilemapRenderer _tilemapRenderer;

	private readonly System.Threading.Lock _mapLoadLock = new();

	public TiledManager(GraphicsDevice graphics, ContentManager content, string start_room)
	{
		_tilemapRenderer = new TilemapRenderer();
		LoadMaps(graphics, content);
		LoadMap(start_room);
	}

	private void LoadMaps(GraphicsDevice graphics, ContentManager content)
	{
		_rooms = new Dictionary<string, TilemapRoom>();

		var loader = Loader.Default();

		string[] files = Directory.GetFiles("Content/Tiled/Maps/", "*.tmx");

		foreach (string path in files)
		{
			FileInfo file = new(path);
			string file_name = file.FullName;

			Map map = loader.LoadMap(file_name);

			LoadTilesetTextures(graphics, content, map);

			_rooms.Add(map.Class, GenerateMap(map));
		}
	}

	private void LoadTilesetTextures(GraphicsDevice graphics, ContentManager content, Map map)
	{
		_tilemapRenderer.ClearTilesets();

		foreach (var tileset in map.Tilesets)
		{
			using (var stream = content.OpenStream(".\\Tiled\\" + tileset.Image.Value.Source.Value))
			{
				Texture2D tileset_texture = Texture2D.FromStream(graphics, stream);

				_tilemapRenderer.AddTileset(tileset.Image.Value.Source.Value, tileset_texture);
			}
		}
	}

	public void Update(GameTime gameTime)
	{
		CheckLoadMap();
		_tilemapRenderer.Update(gameTime);
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

		if (_rooms.ContainsKey(map_name))
		{			
			currentRoomName = map_name;
			currentRoom!.PlayMusic();
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

	private TilemapRoom GenerateMap(Map map)
	{
		Dictionary<string, Collider> collision_rectangles = [];

		LayerIndices layer_indices = new();

		for (int i = 0; i < map.Layers.Count; i++)
		{
			switch (map.Layers[i].Name.ToLower())
			{
				case "floor layer":
					layer_indices.floorTileLayerIndex = i;
					break;

				case "wall layer":
					layer_indices.wallTileLayerIndex = i;
					break;

				case "interaction tile layer":
					layer_indices.interactionTileLayerIndex = i;
					break;

				case "collision layer":
					layer_indices.collisionLayerIndex = i;
					break;

				case "trigger layer":
					layer_indices.triggerLayerIndex = i;
					break;

				case "interaction layer":
					layer_indices.interactionLayerIndex = i;
					break;
			}

			if (layer_indices.allLayersDefined()) 
			{
				break;
			}
		}

		if (layer_indices.collisionLayerIndex.HasValue)
		{
			foreach (var obj in ((ObjectLayer)map.Layers[layer_indices.collisionLayerIndex.Value]).Objects)
			{
				bool enabled = true;
				string name = obj.Name != "" ? obj.Name : obj.ID.Value.ToString();

				if (obj.TryGetProperty<BoolProperty>("enabled", out var enabled_value))
				{
					enabled = enabled_value.Value;
				}

				collision_rectangles.Add(name, new Collider(new RectangleF(obj.X, obj.Y, obj.Width, obj.Height), enabled));
			}
		}

		List<Trigger> triggers = [];

		if (layer_indices.triggerLayerIndex.HasValue)
		{
			foreach (var obj in ((ObjectLayer)map.Layers[layer_indices.triggerLayerIndex.Value]).Objects)
			{
				List<Function>? on_enter = [];
				List<Function>? on_exit = [];
				List<Function>? on_stay = [];
				Color color = Color.White;
				
				if (obj.TryGetProperty<StringProperty>("onEnter", out var enter_code))
				{
					on_enter = FunctionProcessor.Process($"Trigger {obj.ID.Value}", enter_code.Value);
				}

				if (obj.TryGetProperty<StringProperty>("onExit", out var exit_code))
				{
					on_exit = FunctionProcessor.Process($"Trigger {obj.ID.Value}", exit_code.Value);
				}

				if (obj.TryGetProperty<StringProperty>("onStay", out var stay_code))
				{
					on_stay = FunctionProcessor.Process($"Trigger {obj.ID.Value}", stay_code.Value);
				}

				if (obj.TryGetProperty<ColorProperty>("color", out var c))
				{
					color = new Color(c.Value.Value.R, c.Value.Value.G, c.Value.Value.B, c.Value.Value.A);
				}

				triggers.Add(new Trigger(obj.ID.Value, new Rectangle((int)obj.X, (int)obj.Y, (int)obj.Width, (int)obj.Height), on_enter, on_exit, on_stay, color));
			}
		}

		List<InteractionTile> interaction_tiles = [];

		if (layer_indices.interactionLayerIndex.HasValue)
		{
			foreach (var obj in ((ObjectLayer)map.Layers[layer_indices.interactionLayerIndex.Value]).Objects)
			{
				List<Function>? on_interact = [];
				List<Function>? on_uninteract = [];
				List<Function>? while_interact = [];
				List<Function>? on_toggle_on = [];
				List<Function>? on_toggle_off = [];
				bool default_toggle_state = false;
				bool collides = true;
				
				if (obj.TryGetProperty<StringProperty>("onInteract", out var enter_code))
				{
					on_interact = FunctionProcessor.Process($"Interaction {obj.ID.Value}", enter_code.Value);
				}
				if (obj.TryGetProperty<StringProperty>("onUninteract", out var exit_code))
				{
					on_uninteract = FunctionProcessor.Process($"Interaction {obj.ID.Value}", exit_code.Value);
				}
				if (obj.TryGetProperty<StringProperty>("whileInteract", out var stay_code))
				{
					while_interact = FunctionProcessor.Process($"Interaction {obj.ID.Value}", stay_code.Value);
				}
				if (obj.TryGetProperty<StringProperty>("onToggleOn", out var on_code))
				{
					on_toggle_on = FunctionProcessor.Process($"Interaction {obj.ID.Value}", on_code.Value);
				}
				if (obj.TryGetProperty<StringProperty>("onToggleOff", out var off_code))
				{
					on_toggle_off = FunctionProcessor.Process($"Interaction {obj.ID.Value}", off_code.Value);
				}
				if (obj.TryGetProperty<BoolProperty>("defaultToggleState", out var state))
				{
					default_toggle_state = state.Value;
				}
				if (obj.TryGetProperty<BoolProperty>("collides", out var collide))
				{
					collides = collide.Value;
				}

				interaction_tiles.Add(new InteractionTile(obj.ID.Value,
														new Rectangle((int)obj.X, (int)obj.Y, (int)obj.Width, (int)obj.Height), 
														on_interact: on_interact, 
														on_uninteract: on_uninteract, 
														while_interact: while_interact,
														on_toggle_on: on_toggle_on,
														on_toggle_off: on_toggle_off,
														default_toggle_state: default_toggle_state));

				if (collides)
				{
					string name = obj.Name != "" ? obj.Name : obj.ID.Value.ToString();
					collision_rectangles.Add(name, new Collider(new RectangleF((int)obj.X, (int)obj.Y, (int)obj.Width, (int)obj.Height), true));
				}
			}
		}

		string? bgm_name = null;
		float? bgm_volume = null;
		if (map.TryGetProperty<StringProperty>("bgm", out var bgm_name_prop))
		{
			bgm_name = bgm_name_prop.Value;

			if (bgm_name != null)
			{
				if (map.TryGetProperty<FloatProperty>("bgmVolume", out var bgm_volume_prop))
				{
					bgm_volume = Math.Clamp(bgm_volume_prop.Value, 0, 1);
				}
			}
		}

		return new TilemapRoom(map, layer_indices, collision_rectangles, interaction_tiles, triggers, bgm_name, bgm_volume);
	}

	public Vector2 TilePosToWorldPos(Point tile)
	{
		return TilePosToWorldPos(tile.X, tile.Y);
	}

	public Vector2 TilePosToWorldPos(int x, int y)
	{
		return new Vector2(x * currentRoom!.map.TileWidth, y * currentRoom.map.TileHeight);
	}

	public RelativeVector2 TilePosToWorldPos(RelativeVariable<int> x, RelativeVariable<int> y)
	{
		return new RelativeVector2(new RelativeVariable<float>(x.Value * currentRoom!.map.TileWidth, x.Relative), new RelativeVariable<float>(y.Value * currentRoom.map.TileHeight, y.Relative));
	}

	public RelativeVector2 TilePosToWorldPos(RelativeVariable<float> x, RelativeVariable<float> y)
	{
		return new RelativeVector2(new RelativeVariable<float>(x.Value * currentRoom!.map.TileWidth, x.Relative), new RelativeVariable<float>(y.Value * currentRoom.map.TileHeight, y.Relative));
	}

	public RelativeVector2 TilePosToWorldPos(RelativeVariable<FunctionParameter<int>> x, RelativeVariable<FunctionParameter<int>> y)
	{
		var x_value = x.Value.Value;
		var y_value = y.Value.Value;

		if (!x_value.HasValue || !y_value.HasValue)
		{
			return new RelativeVector2(new RelativeVariable<float>(0, x.Relative), new RelativeVariable<float>(0, y.Relative));
		}

		return new RelativeVector2(new RelativeVariable<float>(x_value.Value * currentRoom!.map.TileWidth, x.Relative), new RelativeVariable<float>(y_value.Value * currentRoom.map.TileHeight, y.Relative));
	}

	public Point WorldPosToTilePos(Point point)
	{
		return point / new Point(currentRoom!.map.TileWidth, currentRoom.map.TileHeight);
	}

	public Vector2 WorldPosToTilePos(Vector2 v2)
	{
		return v2 / new Vector2(currentRoom!.map.TileWidth, currentRoom.map.TileHeight);
	}

	public float WorldXToTileX(float x)
	{
		return x / currentRoom!.map.TileWidth;
	}

	public float WorldYToTileY(float y)
	{
		return y / currentRoom!.map.TileHeight;
	}

	public float TileXToWorldX(float x)
	{
		return x * currentRoom!.map.TileWidth;
	}

	public float TileYToWorldY(float y)
	{
		return y * currentRoom!.map.TileHeight;
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		if (currentRoom == null)
		{
			return;
		}

		_tilemapRenderer.Draw(spriteBatch, currentRoom.map, Game1.camera.BoundingRectangle);
	}
}