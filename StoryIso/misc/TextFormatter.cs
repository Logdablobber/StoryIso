using System;
using System.Collections.Generic;
using MonoGame.Extended;
using System.Text.RegularExpressions;
using MonoGame.Extended.BitmapFonts;

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

	public static List<string> FitText(string text, BitmapFont font, float font_scale, SizeF size, out float scale_mult, bool combine_words = true)
	{
		List<string> wrapped_text = WrapText(text, font, font_scale, size.Width, combine_words);
		scale_mult = 1f;

		while (font.MeasureString(string.Join('\n', wrapped_text)).Height * font_scale > size.Height)
		{
			scale_mult *= 0.98f;

			wrapped_text = WrapText(text, font, font_scale * scale_mult, size.Width, combine_words);
		}

		return wrapped_text;
	}

	public static List<string> WrapText(string text, BitmapFont font, float font_scale, float width, bool combine_words = true)
	{
		if (text.Contains('\n'))
		{
			List<string> lines = [];

			foreach (var line in SplitLines(text))
			{
				lines.AddRange(WrapText(line, font, font_scale, width, combine_words));
			}

			return lines;
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

			for (int i = 0; i < words.Length; i++)
			{
				string word = words[i].ToString();

				float word_width = font.MeasureString(word).Width;

				if (word_width > relative_width) // split up word
				{
					float hyphen_width = font.MeasureString("-").Width;

					float first_char_width = font.MeasureString(word[0].ToString()).Width;

					if (line_width + space_width + first_char_width > relative_width)
					{
						line_width = 0f;
						wrapped_lines.Add(word[0].ToString());
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

		return wrapped_lines;
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