using System;

namespace EntropyEditor.Models;

public struct BasicRect
{
	public int X { get; set; }
	public int Y { get; set; }
	public int Width { get; set; }
	public int Height { get; set; }

	public readonly BasicPoint Position => new(X, Y);

	public BasicRect(int x, int y, int width, int height)
	{
		X = x;
		Y = y;
		Width = width;
		Height = height;
	}

	public void IncreaseHeight(int amount)
	{
		this.Height += amount;
	}
}
