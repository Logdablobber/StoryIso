using System;

namespace StoryIso.Misc;

public static class NullableExtensions
{
	public static Optional<T> ToOptional<T>(this T? value) where T : struct
	{
		if (!value.HasValue)
		{
			return default;
		}

		return new Optional<T>(value.Value);
	}
}