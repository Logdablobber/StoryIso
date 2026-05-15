using System;
using Avalonia;

namespace EntropyEditor.Models;

public class RefPoint
{
	public Point Value;

	public RefPoint(Point point)
	{
		this.Value = point;
	}

	public RefPoint(float x, float y)
	{
		this.Value = new Point(x, y);
	}

	public RefPoint(double x, double y)
	{
		this.Value = new Point(x, y);
	}
}
