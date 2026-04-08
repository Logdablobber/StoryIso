using System;
using System.Collections.Generic;
using MonoGame.Extended;
using System.Text.RegularExpressions;
using MonoGame.Extended.BitmapFonts;
using System.Linq;
using Assimp.Configs;
using System.Runtime.CompilerServices;

namespace StoryIso.Misc;

public static class TextFormatter
{
	// gets a range of characters in range [start, end)
	// the indexes of characters in the string exclusions are not counted
	// but they are returned
	public static string RangeExcluding(string text, string exclusions, uint start = 0, uint? end = null)
	{
		if (end.HasValue)
		{
			if (end.Value <= start)
			{
				return "";
			}
		}

		string res = "";

		uint index = 0;

		foreach (char c in text)
		{
			if (end.HasValue && index >= end)
			{
				return res;
			}

			if (index >= start)
			{
				res += c;
			}

			if (exclusions.Contains(c))
			{
				continue;
			}

			index += 1;
		}

		return res;
	}

	public static string[] SplitLines(string text)
	{
		return Regex.Split(text, "\r\n|\r|\n");
	}

	// TODO: make cache clear over time so it doesn't get too large?
	static readonly Dictionary<TextInstance, (ushort[], float)> cached_fitted_text = [];

	private static void Cache(TextInstance instance, string[] lines, float scale_mult)
	{
		ushort[] line_lengths = (from line in lines select (ushort)line.Length).ToArray();

		cached_fitted_text[instance] = (line_lengths, scale_mult);
	}

	private static bool TryGetCache(TextInstance instance, string text, out string[] lines, out float scale_mult)
	{
		if (!cached_fitted_text.TryGetValue(instance, out var value))
		{
			lines = [];
			scale_mult = default;
			return false;
		}

		lines = new string[value.Item1.Length];

		ushort index = 0;

		for (int i = 0; i < value.Item1.Length; i++)
		{
			lines[i] = text[index..(index + value.Item1[i])];
		}

		scale_mult = value.Item2;
		return true;
	}

	public static string[] FitText(string text, FontInstance font, float font_scale, SizeF size, out float scale_mult, bool combine_words = true)
	{
		var instance = new TextInstance(text, font, font_scale, size);

		if (TryGetCache(instance, text, out var lines, out scale_mult))
		{
			return lines;
		}

		string[] wrapped_text = WrapText(text, font.Font, font_scale, size.Width, combine_words);
		scale_mult = 1f;

		while (font.Font.MeasureString(string.Join('\n', wrapped_text)).Height * font_scale * scale_mult > size.Height)
		{
			scale_mult *= 0.98f;

			wrapped_text = WrapText(text, font.Font, font_scale * scale_mult, size.Width, combine_words);
		}

		Cache(instance, lines, scale_mult);

		return wrapped_text;
	}

	public static string[] WrapText(string text, BitmapFont font, float font_scale, float width, bool combine_words = true)
	{
		if (text.Contains('\n'))
		{
			List<string> lines = [];

			foreach (var line in SplitLines(text))
			{
				lines.AddRange(WrapText(line, font, font_scale, width, combine_words));
			}

			return lines.ToArray();
		}

		float relative_width = width / font_scale;
		int wrap_count = (int)Math.Floor(font.MeasureString(text).Width / relative_width);

		if (wrap_count == 0)
		{
			return [text];
		}

		List<string> wrapped_lines = [];

		if (combine_words)
		{
			string[] words = text.Split(' ');

			float line_width = 0f;
			float space_width = font.MeasureString(" ").Width;
			float hyphen_width = font.MeasureString("-").Width;

			for (int i = 0; i < words.Length; i++)
			{
				string word = words[i].ToString();

				float word_width = font.MeasureString(word).Width;

				if (word_width > relative_width) // split up word
				{
					float first_char_width = font.MeasureString(word[0].ToString()).Width;

					if (first_char_width >= relative_width)
					{
						// I wrote this while half asleep so it probably isn't the best :p
						// Splits up words if each letter is big enough to take up an entire line
						wrapped_lines.Add(word[0].ToString());

						string temp_group = "";
						for (int j = 1; j < word.Length; j++)
						{
							if (!string.IsNullOrEmpty(temp_group))
							{
								if (font.MeasureString(temp_group + word[j]).Width >= relative_width)
								{
									wrapped_lines.Add(temp_group);

									temp_group = word[j].ToString();
									continue;
								}

								temp_group += word[j];
								continue;
							}

							if (font.MeasureString(word[j].ToString()).Width >= relative_width)
							{
								wrapped_lines.Add(word[j].ToString());
								continue;
							}

							temp_group = word[j].ToString();
						}

						continue;
					}

					if (line_width + space_width + first_char_width > relative_width)
					{
						line_width = 0f;
						wrapped_lines.Add(word[0].ToString());
					}
					else if (wrapped_lines.Count == 0)
					{
						wrapped_lines.Add(word[0].ToString());
						line_width += space_width + first_char_width;
					}
					else
					{
						wrapped_lines[^1] += " " + word[0];
						line_width += space_width + first_char_width;
					}

					for (int j = 1; j < word.Length; j++)
					{
						string c = word[j].ToString();

						float char_width = font.MeasureString(c).Width;

						// if the letter can fit
						// also check, if it isn't the last letter, if you can also fit a hyphen
						// to allow for hyphenation on long words
						if ((j == word.Length - 1 && line_width + char_width < relative_width) ||
							line_width + char_width + hyphen_width < relative_width)
						{
							wrapped_lines[^1] += c;
							line_width += char_width;
						}
						else
						{
							wrapped_lines[^1] += '-';
							wrapped_lines.Add(c);
							line_width = char_width;
						}
					}

					continue;
				}

				if (wrapped_lines.Count == 0 || 
					line_width + word_width + space_width > relative_width)
				{
					wrapped_lines.Add(word);
					line_width = word_width;
					continue;
				}

				wrapped_lines[^1] += ' ' + word;
				line_width += word_width + space_width;
			}
		}
		else
		{
			float line_width = 0f;

			for (int i = 0; i < text.Length; i++)
			{
				string c = text[i].ToString();
				float char_width = font.MeasureString(c).Width;

				if (wrapped_lines.Count == 0 ||
					line_width + char_width > relative_width)
				{
					wrapped_lines.Add(c);
					line_width = 0f;
					continue;
				}

				wrapped_lines[^1] += c;
				line_width += char_width;
			}
		}

		return wrapped_lines.ToArray();
	}

	const char delim = '\t';

	public static string JoinList(List<string> list)
	{
		return string.Join(delim, list);
	}

	public static string[] SeparateList(string list)
	{
		return list.Split(delim);
	}
}