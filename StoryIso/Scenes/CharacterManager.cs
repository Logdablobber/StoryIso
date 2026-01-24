using System.IO;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using Newtonsoft.Json;
using StoryIso.Entities;
using StoryIso.Enums;
using StoryIso.Misc;

namespace StoryIso.Scenes;

public static class CharacterManager
{
	public static void LoadCharacters(string path, World world)
	{
		string[] files = Directory.GetFiles(path, "*.json");

		foreach (string f in files)
		{
			string text;
			using (StreamReader streamReader = new StreamReader(f))
			{
				text = streamReader.ReadToEnd();
			}

			var data = JsonConvert.DeserializeObject<CharacterData>(text);

			Animation animation = AsepriteLoader.GetAnimation(data.animation);

			var new_character = world.CreateEntity();
			new_character.Attach(animation);
			new_character.Attach(new Transform2(new Vector2(0, 0), 0, Game1.characterScale));
			new_character.Attach(new Character(data.name, Direction.Down, false));
		}
	}
}