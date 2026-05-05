using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Entropy.Misc;

public static class ArrayExtensions
{
	public static Vector2 ToVector2(this float[] obj)
	{
		if (obj.Length != 2)
		{
			throw new IndexOutOfRangeException();
		}

		return new Vector2(obj[0], obj[1]);
	}

	public static SizeF ToSizeF(this float[] obj)
	{
		if (obj.Length != 2)
		{
			throw new IndexOutOfRangeException();
		}

		return new SizeF(obj[0], obj[1]);
	}
}