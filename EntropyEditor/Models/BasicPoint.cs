using System;
using System.ComponentModel;
using Avalonia;

namespace EntropyEditor.Models;

public struct BasicPoint
{
	private int _x;
	private int _y;

	public int X => _x;
	public int Y => _y;

	public BasicPoint(int x, int y)
	{
		_x = x;
		_y = y;
	}

	public void SetX(int x)
	{
		_x = x;
	}

	public void SetY(int y)
	{
		_y = y;
	}

	public override string ToString()
	{
		return $"({X}, {Y})";
	}

	public Point ToPoint()
	{
		return new Point(X, Y);
	}
}
