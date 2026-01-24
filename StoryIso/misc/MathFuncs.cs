using System;

namespace StoryIso.Misc;

public static class MathFuncs
{
	// returns an interpolated value in between min and max
	// when value = 0, returns min
	// when value = 1, returns max
	// 0 <= value <= 1
	// min <= max
	public static float Lerp(float min, float max, float value)
	{
		return (max - min) * value + min;
	}
}