using System;
using Avalonia;

namespace EntropyEditor.Models;

public struct TilePosition
{
	public int ZIndex { get; set; }
	public BasicPoint Position { get; set; }	

	public TilePosition(BasicPoint position, int zIndex)
	{
		Position = position;
		ZIndex = zIndex;
	}
}
