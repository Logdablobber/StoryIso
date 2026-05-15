using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Avalonia;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using DotTiled;
using EntropyEditor.Models;

namespace EntropyEditor.ViewModels;

public partial class TilemapViewModel : ViewModelBase
{
	public const float TileSize = 49.9f; // slight overlap to remove clipping

	private TilemapDataViewModel _tilemap;
	public TilemapDataViewModel Tilemap
	{
		get => _tilemap;
		set => SetProperty(ref _tilemap, value);
	}

	public bool IsDragging { get; set; } = false;

	private Dictionary<string, int> _layers = new Dictionary<string, int>();

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	public TilemapViewModel()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	{
		Tilemap = TilemapReader.LoadFromFile("avares://EntropyEditor/Assets/Tilemaps/Maps/test-tilemap.tmx", out var bgm, out var layers);
	}

	public void AddTile(BasicPoint position, string layer, uint tileID, string tilesetName)
	{
		var tile_pos = new TilePosition() { ZIndex = _layers[layer], Position = position };

		Tilemap[tile_pos] = new TileViewModel(tile_pos, tileID, tilesetName, Tilemap.Offset);
	}

	const int ScrollAmount = 5;
	public void ScrollLeft()
	{
		if (IsDragging)
		{
			return;
		}

		Tilemap.Offset = new RefPoint(Tilemap.Offset.Value.X + ScrollAmount, Tilemap.Offset.Value.Y);
	}

	public void ScrollRight()
	{
		if (IsDragging)
		{
			return;
		}

		Tilemap.Offset = new RefPoint(Tilemap.Offset.Value.X - ScrollAmount, Tilemap.Offset.Value.Y);
	}

	public void ScrollUp()
	{
		if (IsDragging)
		{
			return;
		}

		Tilemap.Offset = new RefPoint(Tilemap.Offset.Value.X, Tilemap.Offset.Value.Y + ScrollAmount);
	}

	public void ScrollDown()
	{
		if (IsDragging)
		{
			return;
		}

		Tilemap.Offset = new RefPoint(Tilemap.Offset.Value.X, Tilemap.Offset.Value.Y - ScrollAmount);
	}

	public BasicPoint GetTileAtPoint(Point point)
	{
		point -= Tilemap.Offset.Value;

		point /= TileSize;

		return point.ToBasicPoint();
	}
}
