using MonoGame.Extended;
using StoryIso.Entities;

namespace StoryIso.Misc;


// the text is converted into an array of the sizes of the characters
// bc only the size matters when fitting an array
// this makes caching more efficient
public struct TextInstance
{
	public uint[] text;
	public SizeF size;

	public TextInstance(string text, FontInstance font, float font_scale, SizeF size)
	{
		this.text = new uint[text.Length];

		for (int i = 0; i < text.Length; i++)
		{
			var c = font.Font.GetCharacter(text[i]);

			ushort height = (ushort)c.TextureRegion.Height;
			ushort width = (ushort)c.TextureRegion.Width;

			this.text[i] = (uint)((width << 16) | height);
		}

		this.size = size / font_scale;
	}
}