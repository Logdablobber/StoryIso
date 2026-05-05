using System.Collections.Generic;

namespace Entropy.Misc;

public static class StringExtension
{
	public static string JoinToString(this string[] s)
	{
		return string.Join(' ', s);
	}

	public static string JoinToString(this string[] s, string delim = " ")
	{
		return string.Join(delim, s);
	}

	public static string JoinToString(this string[] s, char delim = ' ')
	{
		return string.Join(delim, s);
	}

	public static string JoinToString(this List<string> s)
	{
		return string.Join(' ', s);
	}

	public static string JoinToString(this List<string> s, string delim = " ")
	{
		return string.Join(delim, s);
	}

	public static string JoinToString(this List<string> s, char delim = ' ')
	{
		return string.Join(delim, s);
	}
}