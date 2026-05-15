using System;
using System.IO;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using DotTiled;

namespace EntropyEditor.Models;

public class Tileset
{
	public Bitmap bitmap { get; set; }
	private readonly double _tileSizeX;
	private readonly double _tileSizeY;
	private readonly int _tilesPerRow;

	public RelativeRect GetRelativeRect(uint index)
	{
		double x = index % _tilesPerRow * _tileSizeX;
		double y = index / _tilesPerRow * _tileSizeY;
		return new RelativeRect(x, y, _tileSizeX, _tileSizeY, RelativeUnit.Relative);
	}

	public Tileset(DotTiled.Tileset tileset)
	{
		var relative_path = Path.GetRelativePath(Directory.GetCurrentDirectory(), tileset.Image.Value.Source.Value);
		using var resource = AssetLoader.Open(new Uri(Path.Combine(TilemapReader.basePath, relative_path)));

		bitmap = new Bitmap(resource);
		_tilesPerRow = tileset.Columns;

		_tileSizeX = 1.0d / _tilesPerRow;
		_tileSizeY = 1.0d / Math.Ceiling((double)tileset.TileCount / tileset.Columns);
	}
}
