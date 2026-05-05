using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using Entropy.Misc;

namespace Entropy.FileLoading;

public static class FontLoader
{
	private static readonly Dictionary<string, FontInstance> _fonts = [];

	public static FontInstance? GetFont(string name)
	{
		if (_fonts.TryGetValue(name, out var value))
		{
			return value;
		}

		return null;
	}

	public static void LoadFonts(GraphicsDevice graphics, string path)
	{
		_fonts.Clear();

		string[] files = Directory.GetFiles(path, "*.fnt");

		foreach (string f in files)
		{
			BitmapFont font_file = BitmapFont.FromFile(graphics, f);

			FileInfo file = new(f);

			string font_name = file.Name.Replace(".fnt", null);

			_fonts.Add(font_name, new FontInstance(font_name, font_file));
		}
	}
}