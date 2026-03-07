using System.Collections.Generic;
using System.IO;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.IO;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Aseprite;
using StoryIso.Entities;

namespace StoryIso.FileLoading;

public static class AsepriteLoader
{
	private static readonly Dictionary<string, Animation> _animations = [];

	public static Animation? GetAnimation(string name)
	{
		if (_animations.TryGetValue(name, out var value))
		{
			return value.Clone();
		}

		return null;
	}

	public static void LoadAsefiles(GraphicsDevice graphics, string path)
	{
		_animations.Clear();

		string[] files = Directory.GetFiles(path, "*.aseprite");

		foreach (string f in files)
		{
			AsepriteFile asefile = LoadAsefile(f);

			MonoGame.Aseprite.SpriteSheet spriteSheet;
			spriteSheet = asefile.CreateSpriteSheet(graphics, true);

			FileInfo file = new(f);

			string animation_name = file.Name.Replace(".aseprite", null);

			_animations.Add(animation_name,  new Animation(spriteSheet, "Standing Down"));
		}
	}

	private static AsepriteFile LoadAsefile(string path)
	{
		AsepriteFile aseFile;
		aseFile = AsepriteFileLoader.FromFile(path, false);
 
		return aseFile;
	} 
}