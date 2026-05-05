using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using MonoGame.Extended;
using Entropy.Entities;

namespace Entropy.Misc;


// the text is converted into an array of the sizes of the characters
// bc only the size matters when fitting an array
// this makes caching more efficient
public struct TextInstance
{
	public uint[] text;
	public bool wrap_text;
	public SizeF size;

	public TextInstance(string text, FontInstance font, float font_scale, SizeF size, bool wrap_text)
	{
		// TODO: rewrite this to allow for multiline strings
		if (wrap_text)
		{
			this.text = new uint[text.Length];

			for (int i = 0; i < text.Length; i++)
			{
				var c = font.Font.GetCharacter(text[i]);

				if (c == null)
				{
					continue;
				}

				ushort height = (ushort)c.TextureRegion.Height;
				ushort width = (ushort)c.TextureRegion.Width;

				this.text[i] = (uint)((width << 16) | height);
			}
		}
		else
		{
			uint length = 0;
			uint height = 0;

			for (int i = 0; i < text.Length; i++)
			{
				var c = font.Font.GetCharacter(text[i]);

				if (c.TextureRegion.Height > height)
				{
					height = (uint)c.TextureRegion.Height;
				}

				length += (uint)c.TextureRegion.Width;
			}

			this.text = [length, height];
		}

		this.size = size / font_scale;
		this.wrap_text = wrap_text;
	}

	public override readonly bool Equals([NotNullWhen(true)] object? obj)
	{
		if (obj is TextInstance textInstance)
		{
			if (textInstance.wrap_text != this.wrap_text ||
				textInstance.size != this.size ||
				textInstance.text != this.text)
			{
				return false;
			}

			return true;
		}

		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		int hashcode = 0;

		foreach (var item in text)
		{
			hashcode ^= (int)item;
		}

		return hashcode ^ wrap_text.GetHashCode() ^ size.GetHashCode();
	}
}