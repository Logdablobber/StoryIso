using System;
using Avalonia;
using Avalonia.Media;

namespace EntropyEditor.Models;

public static class AvaloniaExtensions
{
	public static Color ToAvaloniaColor(this DotTiled.TiledColor color)
	{
		return Color.FromArgb(color.A, color.R, color.G, color.B);
	}

	public static BasicPoint ToBasicPoint(this Point point)
	{
		return new BasicPoint((int)point.X, (int)point.Y);
	}
}
