using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Rendering.Composition.Animations;
using DotTiled;
using EntropyEditor.Models;

namespace EntropyEditor.ViewModels;

public class TilemapDataViewModel : IList<ITilemapItem>, IList, INotifyCollectionChanged, INotifyPropertyChanged
{
	private int _tilemapWidth { get; set; }

	private Dictionary<TilePosition, TileViewModel> _tiles { get; set; }
	private Dictionary<TilePosition, ITilemapItem> _meshedTiles { get; set; }
	private List<ITilemapItem> _objects { get; set; }

	private readonly RefPoint _offset;

	public RefPoint Offset
	{
		get => _offset;
		set
		{
			_offset.Value = value.Value;

			// Don't need to update this bc they're not rendered
			// only meshed tiles and objects are rendered
			/*foreach (var (_, tile) in _tiles)
			{
				tile.OnOffsetChanged();
			}*/

			foreach (var (_, meshedTile) in _meshedTiles)
			{
				meshedTile.OnOffsetChanged();
			}

			foreach (var obj in _objects)
			{
				obj.OnOffsetChanged();
			}
 		}
	}

	public bool IsDragging { get; set; } = false;
	public Point DragStart { get; set; }

	public int Count => _tiles.Count;

	public bool IsReadOnly => false;

	public bool IsFixedSize => false;

	public bool IsSynchronized => false;

	public object SyncRoot => throw new NotImplementedException();

	object? IList.this[int index]
	{
		get
		{
			return _tiles.Values.ElementAt(index);
		}
		set => throw new NotImplementedException(); 
	}

	public ITilemapItem this[int index]
	{
		get 
		{
			return _tiles.Values.Concat(_objects).ElementAt(index);
		}
		set => throw new NotImplementedException(); 
	}

	public TilemapDataViewModel(RefPoint offset, int tilemapWidth)
	{
		_tiles = [];
		_objects = [];
		_meshedTiles = [];
		_offset = offset;
		_tilemapWidth = tilemapWidth;
	}

	public TileViewModel this[TilePosition position, bool repressUpdate = false]
	{
		get => _tiles[position];
		set
		{
			bool replace = _tiles.TryGetValue(position, out var old);
			_tiles[position] = value;

			if (replace)
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs($"{nameof(_tiles)}[{position}]"));

				if (CollectionChanged != null)
				{
					var e = new NotifyCollectionChangedEventArgs(
						NotifyCollectionChangedAction.Replace,
						value, old);
					CollectionChanged(this, e);
				}
			}
			else
			{

				NotifyAdd(value);
			}

			if (repressUpdate) // if updating in batch, only the final update should re-mesh the tiles
			{
				return;
			}

			GreedyMeshTiles();
		}
	}

	public event NotifyCollectionChangedEventHandler? CollectionChanged;
	public event PropertyChangedEventHandler? PropertyChanged;

	private void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	private void OnCollectionChanged(NotifyCollectionChangedEventArgs collectionChangeEventArgs)
	{
		CollectionChanged?.Invoke(this, collectionChangeEventArgs);
	}

	public void Add(ITilemapItem item)
	{
		if (item is TileViewModel tile)
		{
			if (_tiles.TryAdd(item.Position, tile))
			{
				NotifyAdd(item);
				GreedyMeshTiles();
				return;
			}

			throw new ArgumentException($"A tile already exists at position {item.Position}");
		}

		if (item is TilemapObjectViewModel obj)
		{
			_objects.Add(obj);
			NotifyAdd(obj);
			return;
		}

		throw new NotImplementedException($"Adding items of type {item.GetType()} is not supported");
	}

	public void SetTile(TilePosition position, uint id, string tileset)
	{
		this[position] = new TileViewModel(position, id, tileset, Offset);
	}

	public void Clear()
	{
		_tiles.Clear();
		_objects.Clear();
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}

	public bool Contains(ITilemapItem item)
	{
		if (item is TileViewModel tile)
		{
			return _tiles.TryGetValue(item.Position, out var existingTile) && existingTile == tile;
		}

		if (item is TilemapObjectViewModel obj)
		{
			return _objects.Contains(obj);
		}

		throw new NotImplementedException($"Checking containment for items of type {item.GetType()} is not supported");
	}

	public void CopyTo(ITilemapItem[] array, int arrayIndex)
	{
		throw new NotImplementedException();
	}

	public bool Remove(ITilemapItem item)
	{
		if (item is TileViewModel tile)
		{
			if (_tiles.Remove(item.Position))
			{
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, tile));
				GreedyMeshTiles();
				return true;
			}

			return false;
		}

		if (item is TilemapObjectViewModel obj)
		{
			if (_objects.Remove(obj))
			{
				OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, obj));
				return true;
			}

			return false;
		}

		return false;
	}
	
	public void GreedyMeshTiles()
	{
		Dictionary<(string, uint, int), List<BitArray>> tiles = [];

		foreach (var (position, tile) in _tiles)
		{
			if (tiles.TryGetValue((tile.TilesetName, tile.TileID, tile.ZIndex), out var bitArrays))
			{
				while (bitArrays.Count <= position.Position.Y)
				{
					bitArrays.Add(new BitArray(_tilemapWidth, false));
				}

				bitArrays[position.Position.Y].Set(position.Position.X, true);
				continue;
			}

			var new_arrays = new List<BitArray>();

			for (int i = 0; i <= position.Position.Y; i++)
			{
				new_arrays.Add(new BitArray(_tilemapWidth, false));
			}

			new_arrays[position.Position.Y].Set(position.Position.X, true);

			tiles[(tile.TilesetName, tile.TileID, tile.ZIndex)] = new_arrays;
		}

		_meshedTiles.Clear();

		foreach (var ((tilesetName, tileId, zIndex), array) in tiles)
		{
			var rects = GreedyMesher.Mesh(array);

			foreach (var rect in rects)
			{
				var position = new TilePosition()
				{
					ZIndex = zIndex,
					Position = rect.Position
				};

				var meshed_tile = new MeshedTileViewModel(position, tileId, tilesetName, Offset, rect.Width, rect.Height);

				_meshedTiles.Add(position, meshed_tile);
			}
		}

		OnPropertyChanged(nameof(_meshedTiles));

		if (CollectionChanged != null)
		{
			var e = new NotifyCollectionChangedEventArgs(
				NotifyCollectionChangedAction.Reset);
			CollectionChanged(this, e);
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	private void NotifyAdd(ITilemapItem tile)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(_tiles)));

		if (CollectionChanged != null)
		{
			var e = new NotifyCollectionChangedEventArgs(
				NotifyCollectionChangedAction.Add,
				new[] { tile },
				-1);
			CollectionChanged(this, e);
		}
	}

	public int IndexOf(ITilemapItem item)
	{
		return _tiles.Keys.ToArray().IndexOf(item.Position);
	}

	public void Insert(int index, ITilemapItem item)
	{
		throw new NotImplementedException();
	}

	public void RemoveAt(int index)
	{
		throw new NotImplementedException();
	}

	public int Add(object? value)
	{
		if (value is not ITilemapItem tile)
		{
			throw new ArgumentException($"Value must be of type {typeof(ITilemapItem)}");
		}

		Add(tile);
		return IndexOf(tile);
	}

	public bool Contains(object? value)
	{
		return value is ITilemapItem tile && Contains(tile);
	}

	public int IndexOf(object? value)
	{
		return value is ITilemapItem tile ? IndexOf(tile) : -1;
	}

	public void Insert(int index, object? value)
	{
		throw new NotImplementedException();
	}

	public void Remove(object? value)
	{
		if (value is ITilemapItem tile)
		{
			Remove(tile);
		}
		else
		{
			throw new ArgumentException($"Value must be of type {typeof(ITilemapItem)}");
		}
	}

	public void CopyTo(Array array, int index)
	{
		throw new NotImplementedException();
	}

	public IEnumerator<ITilemapItem> GetEnumerator()
	{
		return _meshedTiles.Values.Concat(_objects).GetEnumerator();
	}
}
