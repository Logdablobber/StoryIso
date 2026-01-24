using System.Drawing;

namespace StoryIso.Misc;

public struct RelativePoint
{
	public RelativeVariable<int> X;
	public RelativeVariable<int> Y;

	public RelativePoint(RelativeVariable<int> x, RelativeVariable<int> y)
	{
		X = x;
		Y = y;
	}

	public Point ToAbsolute(Point origin)
	{
		Point res = new Point();

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

	public static RelativePoint operator *(RelativePoint rp, int mult)
	{
		rp.X.Value *= mult;
		rp.Y.Value *= mult;

		return rp;
	}

	public static RelativePoint operator *(RelativePoint rp, Point mult)
	{
		rp.X.Value *= mult.X;
		rp.Y.Value *= mult.Y;

		return rp;
	}
}