using System.Collections.Generic;
using System.Linq;
using MonoGame.Extended.Tiled;
using StoryIso.Debugging;
using StoryIso.Enums;

namespace StoryIso.Tiled;

public class TilemapRoom
{
	public TiledMap map;
	public LayerIndices layerIndices;
	private Dictionary<string, Collider> _collisionRectangles;
	public List<InteractionTile> interactionTiles;
	public List<Trigger> triggers;
	public List<Collider> Colliders { get { return _collisionRectangles.Values.ToList(); } }

	public TilemapRoom(TiledMap map, 
						LayerIndices layer_indices, 
						Dictionary<string, Collider> collision_rectangles,
						List<InteractionTile> interaction_tiles,
						List<Trigger> triggers)
	{
		this.map = map;
		layerIndices = layer_indices;
		_collisionRectangles = collision_rectangles;
		interactionTiles = interaction_tiles;
		this.triggers = triggers;
	}

	public void SetTile(ushort x, ushort y, uint guid, TileLayerType layer)
	{
		switch (layer)
		{
			case TileLayerType.FloorLayer:
				if (!layerIndices.floorTileLayerIndex.HasValue)
				{
					return;
				}

				map.TileLayers[layerIndices.floorTileLayerIndex.Value].SetTile(x, y, guid);
				break;

			case TileLayerType.WallLayer:
				if (!layerIndices.wallTileLayerIndex.HasValue)
				{
					return;
				}
				
				map.TileLayers[layerIndices.wallTileLayerIndex.Value].SetTile(x, y, guid);
				break;

			case TileLayerType.InteractionLayer:
				if (!layerIndices.interactionTileLayerIndex.HasValue)
				{
					return;
				}

				map.TileLayers[layerIndices.interactionTileLayerIndex.Value].SetTile(x, y, guid);
				break;

			default:
				break;
		}
	}

	public void RemoveTile(ushort x, ushort y, TileLayerType layer)
	{
		SetTile(x, y, 0, layer);
	}

	// NOTE: line is only for raising errors
	// this is fairly obvious
	// but I'll probably be confused by it later anyways :\
	public void ToggleCollider(string name, Source source)
	{
		if (_collisionRectangles.TryGetValue(name, out Collider col))
		{
			col.ToggleEnabled();
		}
		else
		{
			DebugConsole.Raise(new UnknownColliderError(source, "ToggleCollider", name));
		}
	}

	public void SetCollider(string name, bool enabled, Source source)
	{
		if (_collisionRectangles.TryGetValue(name, out Collider col))
		{
			col.SetEnabled(enabled);
		}
		else
		{
			DebugConsole.Raise(new UnknownColliderError(source, "SetCollider", name));
		}
	}
}

public struct LayerIndices
{
	public int? collisionLayerIndex;
	public int? triggerLayerIndex;
	public int? interactionLayerIndex;
	public int? floorTileLayerIndex;
	public int? wallTileLayerIndex;
	public int? interactionTileLayerIndex;
}