using System;
using Microsoft.Xna.Framework;

namespace StoryIso.Misc;

public struct RelativeVector2
{
	public RelativeVariable<float> X;
	public RelativeVariable<float> Y;

	public RelativeVector2(RelativeVariable<float> x, RelativeVariable<float> y)
	{
		X = x;
		Y = y;
	}

	public RelativeVector2(Vector2 v2)
	{
		X = new RelativeVariable<float>(v2.X, false);
		Y = new RelativeVariable<float>(v2.Y, false);
	}

	public Vector2 ToAbsolute(Vector2 origin)
	{
		Vector2 res = new Vector2();

		if (X.Relative)
		{
			res.X = origin.X + X.Value;
		}
		else
		{
			res.X = X.Value;
		}

		if (Y.Relative)
		{
			res.Y = origin.Y + Y.Value;
		}
		else
		{
			res.Y = Y.Value;
		}

		return res;
	}

	public static RelativeVector2 operator *(RelativeVector2 rv2, int mult)
	{
		rv2.X.Value *= mult;
		rv2.Y.Value *= mult;

		return rv2;
	}

	public static RelativeVector2 operator *(RelativeVector2 rv2, float mult)
	{
		rv2.X.Value *= mult;
		rv2.Y.Value *= mult;

		return rv2;
	}

	public static RelativeVector2 operator *(RelativeVector2 rv2, Vector2 mult)
	{
		rv2.X.Value *= mult.X;
		rv2.Y.Value *= mult.Y;

		return rv2;
	}

	public void Normalize()
	{
		float length = MathF.Sqrt(X.Value * X.Value + Y.Value * Y.Value);

		X.Value /= length;
		Y.Value /= length;
	}
}