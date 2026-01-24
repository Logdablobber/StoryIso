using System;

namespace StoryIso.Misc;

public static class ParseNullable
{
	extension<T>(T? value)
		where T : struct, IParsable<T>
	{
			public bool TryParse(string? s, out T? result) => 
				T.TryParse(s, null, out result);
	}
}