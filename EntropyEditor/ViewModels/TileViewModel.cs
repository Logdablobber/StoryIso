using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using DotTiled;
using EntropyEditor.Models;

namespace EntropyEditor.ViewModels;

public partial class TileViewModel : ViewModelBase, ITilemapItem
{
	private uint _tileID;
	private string _tilesetName;
	private float _x;
	private float _y;
	private int _zIndex;
	private RefPoint _offset;

	public uint TileID
	{
		get => _tileID;
		set => SetProperty(ref _tileID, value);
	}
	public RelativeRect SourceRect
	{
		get => TilesetManager.GetRelativeRect(TilesetName, _tileID);
	}
	public string TilesetName
	{
		get => _tilesetName;
		set { SetProperty(ref _tilesetName, value); OnPropertyChanged(nameof(Source)); }
	}
	public Bitmap Source
	{
		get => TilesetManager.GetTileset(TilesetName).bitmap;
	}
	public float X
	{
		get => _x + (float)_offset.Value.X;
		set => SetProperty(ref _x, value);
	}
	public float Y
	{
		get => _y + (float)_offset.Value.Y;
		set => SetProperty(ref _y, value);
	}

	public int ZIndex
	{
		get => _zIndex;
		set => SetProperty(ref _zIndex, value);
	}

	public TilePosition Position => new TilePosition() { Position = new BasicPoint((int)Math.Ceiling(X / TilemapViewModel.TileSize), (int)Math.Ceiling(Y / TilemapViewModel.TileSize)), ZIndex = ZIndex };

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	public TileViewModel(TilePosition Position, uint tileID, string tilesetName, RefPoint offset)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	{
		this._offset = offset;
		this.X = (float)(Position.Position.X * TilemapViewModel.TileSize);
		this.Y = (float)(Position.Position.Y * TilemapViewModel.TileSize);
		this.ZIndex = Position.ZIndex;
		this.TileID = tileID;
		this.TilesetName = tilesetName;
	}

	public void OnOffsetChanged()
	{
		OnPropertyChanged(nameof(X));
		OnPropertyChanged(nameof(Y));
	}

	public override string ToString()
	{
		return "Tile";
	}
}
