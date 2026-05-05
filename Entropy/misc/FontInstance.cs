using MonoGame.Extended.BitmapFonts;

namespace Entropy.Misc;

public class FontInstance
{
	public readonly string Name;
	public readonly BitmapFont Font;

	public FontInstance(string name, BitmapFont font)
	{
		this.Name = name;
		this.Font = font;
	}
}