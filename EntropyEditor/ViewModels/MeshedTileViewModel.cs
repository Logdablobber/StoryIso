using System;
using System.Reflection.Metadata.Ecma335;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Embedding.Offscreen;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using DotTiled;
using EntropyEditor.Models;

namespace EntropyEditor.ViewModels;

public partial class MeshedTileViewModel : ViewModelBase, ITilemapItem
{
	private uint _tileID;
	private string _tilesetName;
	private float _x;
	private float _y;
	private float _width;
	private float _height;
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
	public float Width
	{
		get => _width;
		set => SetProperty(ref _width, value);
	}
	public float Height
	{
		get => _height;
		set => SetProperty(ref _height, value);
	}
	public int ZIndex
	{
		get => _zIndex;
		set => SetProperty(ref _zIndex, value);
	}

	public TilePosition Position => new TilePosition() { Position = new BasicPoint((int)Math.Ceiling(X / TilemapViewModel.TileSize), (int)Math.Ceiling(Y / TilemapViewModel.TileSize)), ZIndex = ZIndex };

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	public MeshedTileViewModel(TilePosition Position, uint tileID, string tilesetName, RefPoint offset, int width, int height)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
	{
		this._offset = offset;
		this.X = (float)(Position.Position.X * TilemapViewModel.TileSize);
		this.Y = (float)(Position.Position.Y * TilemapViewModel.TileSize);
		this.ZIndex = Position.ZIndex;
		this.TileID = tileID;
		this.TilesetName = tilesetName;
		this.Width = (float)(width * TilemapViewModel.TileSize);
		this.Height = (float)(height * TilemapViewModel.TileSize);
	}

	public void OnOffsetChanged()
	{
		OnPropertyChanged(nameof(X));
		OnPropertyChanged(nameof(Y));
	}

	public override string ToString()
	{
		return "MeshedTile";
	}
}
