using System;
using System.Collections.Generic;
using Avalonia;

namespace EntropyEditor.Models;

public static class TilesetManager
{
	private static Dictionary<string, Tileset> _tilesets = [];

	public static Tileset GetTileset(string tilesetName)
	{
		if (!_tilesets.TryGetValue(tilesetName, out var tileset))
		{
			throw new KeyNotFoundException($"Tileset '{tilesetName}' not found.");
		}

		return tileset;
	}

	public static RelativeRect GetRelativeRect(string tileset, uint tileID)
	{
		if (!_tilesets.TryGetValue(tileset, out var tilesetData))
		{
			throw new KeyNotFoundException($"Tileset '{tileset}' not found.");
		}

		return tilesetData.GetRelativeRect(tileID);
	}

	public static void AddTileset(string tilesetName, DotTiled.Tileset tileset)
	{
		if (_tilesets.ContainsKey(tilesetName))
		{
			return;
		}

		_tilesets[tilesetName] = new Tileset(tileset);
	}
}
