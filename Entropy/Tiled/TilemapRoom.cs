using System.Collections.Generic;
using System.Linq;
using DotTiled;
using Entropy.Audio;
using Entropy.Debugging;
using Entropy.Enums;

namespace Entropy.Tiled;

public class TilemapRoom
{
	private readonly Map _map;
	public Map map
	{
		get
		{
			lock (_mapLock)
			{
				return _map;
			}
		}
	}
	public LayerIndices layerIndices;

	private readonly string? bgm_name = null;
	private readonly float? bgm_volume = null;
	
	public List<InteractionTile> interactionTiles;

	public List<Trigger> Triggers;

	private readonly Dictionary<string, Collider> _collisionRectangles;
	public List<Collider> Colliders 
	{ 
		get 
		{ 
			lock (_colliderLock)
			{
				return _collisionRectangles.Values.ToList(); 
			}
		} 
	}

	private readonly System.Threading.Lock _colliderLock = new();
	private readonly System.Threading.Lock _mapLock = new();

	public TilemapRoom(Map map, 
						LayerIndices layer_indices, 
						Dictionary<string, Collider> collision_rectangles,
						List<InteractionTile> interaction_tiles,
						List<Trigger> triggers,
						string? bgm_name,
						float? bgm_volume)
	{
		_map = map;
		layerIndices = layer_indices;
		_collisionRectangles = collision_rectangles;
		interactionTiles = interaction_tiles;
		Triggers = triggers;
		this.bgm_name = bgm_name;
		this.bgm_volume = bgm_volume;
	}

	private void _setTile(ushort x, ushort y, uint guid, int layer_index)
	{
		lock (_mapLock)
		{
			((TileLayer)_map.Layers[layer_index]).Data.Value.GlobalTileIDs.Value[x + map.Width * y] = guid;
		}
	}

	public void SetTile(ushort x, ushort y, uint guid, TileLayerType layer)
	{
		int? index = layer switch
		{
			TileLayerType.FloorLayer => layerIndices.floorTileLayerIndex,
			TileLayerType.WallLayer => layerIndices.wallTileLayerIndex,
			TileLayerType.InteractionLayer => layerIndices.interactionTileLayerIndex,
			_ => null
		};

		if (index == null)
		{
			return;
		}

		_setTile(x, y, guid, index.Value);
	}

	public void RemoveTile(ushort x, ushort y, TileLayerType layer)
	{
		SetTile(x, y, 0, layer);
	}

	public void PlayMusic()
	{
		if (bgm_name == null)
		{
			return;
		}

		AudioManager.SetBGM(new Source(0, null, _map.Class), bgm_name, bgm_volume ?? 1f);
		AudioManager.PlayBGM();
	}

	// NOTE: line is only for raising errors
	// this is fairly obvious
	// but I'll probably be confused by it later anyways :\
	public void ToggleCollider(string name, Source source)
	{
		lock (_colliderLock)
		{
			if (_collisionRectangles.TryGetValue(name, out Collider? col))
			{
				col.ToggleEnabled();
			}
			else
			{
				DebugConsole.Raise(new UnknownColliderError(source, "ToggleCollider", name));
			}
		}
	}

	public void SetCollider(string name, bool enabled, Source source)
	{
		lock (_colliderLock)
		{
			if (_collisionRectangles.TryGetValue(name, out Collider? col))
			{
				col.SetEnabled(enabled);
			}
			else
			{
				DebugConsole.Raise(new UnknownColliderError(source, "SetCollider", name));
			}
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

	public bool allLayersDefined() =>
				floorTileLayerIndex.HasValue &&
				wallTileLayerIndex.HasValue &&
				interactionTileLayerIndex.HasValue &&
				collisionLayerIndex.HasValue &&
				triggerLayerIndex.HasValue &&
				interactionLayerIndex.HasValue;
}