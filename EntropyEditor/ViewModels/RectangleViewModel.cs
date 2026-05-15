using System;
using Avalonia;
using Avalonia.Media;
using EntropyEditor.Models;

namespace EntropyEditor.ViewModels;

public class RectangleViewModel : ViewModelBase, ITilemapItem
{
	private float _x;
	private float _y;
	private float _width;
	private float _height;
	private Color _color;
	private Color _borderColor;
	private float _opacity;
	private int _zIndex;
	private float _borderThickness;
	private RefPoint _offset;

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
	public Color Color
	{
		get => _color;
		set => SetProperty(ref _color, value);
	}
	public Color BorderColor
	{
		get => _borderColor;
		set => SetProperty(ref _borderColor, value);
	}
	public float Opacity
	{
		get => _opacity;
		set => SetProperty(ref _opacity, value);
	}
	public int ZIndex
	{
		get => _zIndex;
		set => SetProperty(ref _zIndex, value);
	}
	public float BorderThickness
	{
		get => _borderThickness;
		set => SetProperty(ref _borderThickness, value);
	}

	public TilePosition Position => new TilePosition() { Position = new BasicPoint((int)Math.Ceiling(X / TilemapViewModel.TileSize), (int)Math.Ceiling(Y / TilemapViewModel.TileSize)), ZIndex = ZIndex };

	public RectangleViewModel(RefPoint offset, float x, float y, float width, float height, int zIndex = 0, Color? color = null, Color? borderColor = null, float opacity = 1f, float borderThickness = 1)
	{
		_offset = offset;
		X = x * TilemapViewModel.TileSize / 16; // scale up to tile size, but then reduce bc there are 16 px per tile
		Y = y * TilemapViewModel.TileSize / 16;
		Width = width * TilemapViewModel.TileSize / 16;
		Height = height * TilemapViewModel.TileSize / 16;
		ZIndex = zIndex;
		Color = color ?? Colors.White;
		BorderColor = borderColor ?? Colors.Gray;
		Opacity = opacity;
		BorderThickness = borderThickness;
	}

	public void OnOffsetChanged()
	{
		OnPropertyChanged(nameof(X));
		OnPropertyChanged(nameof(Y));
	}
}
