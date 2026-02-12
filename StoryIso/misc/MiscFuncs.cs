using System;

namespace StoryIso.Misc;

public static class MiscFuncs
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

	// check if two types are similar
	// if they are the same or if they are nullable versions of each other
	public static bool SimilarTypes(Type type1, Type type2)
	{
		Type base_type1 = Nullable.GetUnderlyingType(type1) ?? type1;
		Type base_type2 = Nullable.GetUnderlyingType(type2) ?? type2;

		return base_type1 == base_type2;
	}
}