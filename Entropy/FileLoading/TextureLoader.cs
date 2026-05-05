using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Graphics;

namespace Entropy.FileLoading;

public static class TextureLoader
{
	private static readonly Dictionary<string, Texture2D> _textures = [];

	public static Texture2D? GetTexture(string name)
	{
		if (_textures.TryGetValue(name, out var value))
		{
			return value;
		}

		return null;
	}

	public static void LoadTextures(GraphicsDevice graphics, string path)
	{
		_textures.Clear();

		var dirNames = new Regex("[.](bmp|gif|jpg|png|tif|dds)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		string[] files = Directory.GetFiles(path).Where(dir => dirNames.IsMatch(dir)).ToArray();

		foreach (string f in files)
		{
			Texture2D texture_file = Texture2D.FromFile(graphics, f);

			FileInfo file = new(f);

			string texture_name = dirNames.Replace(file.Name, replacement:string.Empty);

			_textures.Add(texture_name, texture_file);
		}
	}
}