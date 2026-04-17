using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using StoryIso.Debugging;
using StoryIso.Entities;
using StoryIso.Enums;
using StoryIso.FileLoading;

namespace StoryIso.Scenes;

public static class CharacterManager
{
	public static void LoadCharacters(string base_path, World world)
	{
		var dir = new DirectoryInfo(base_path);

		var files = dir.GetFiles("*.json");

		foreach (var f in files)
		{
			if (f.Name == "CharacterSchema.json")
			{
				continue;
			}

			string text;
			using (var streamReader = new StreamReader(f.FullName))
			{
				text = streamReader.ReadToEnd();
			}

			var data = JsonSerializer.Deserialize<CharacterData>(text, Game1.DeserializeOptions);

			Animation? animation = AsepriteLoader.GetAnimation(data.animation);

			if (animation == null)
			{
				DebugConsole.Raise(new MissingAssetError(new Source(0, null, "LoadCharacters"), f.Name, "Missing animation"));
			}

			var new_character = world.CreateEntity();
			new_character.Attach(animation);
			new_character.Attach(new Transform2(new Vector2(0, 0), 0, Game1.characterScale));
			new_character.Attach(new Character(data.name, Direction.Down, data.room));
			new_character.Attach(new RenderAttributes(data.visible, data.color ?? Color.White, RenderLayer.Characters));
		}

		#if DEBUG

		JsonNode schema = Game1.DeserializeOptions.GetJsonSchemaAsNode(typeof(CharacterData));

		var path = Path.GetFullPath("./");

		var src_path = Regex.Replace(path, @"bin.+", string.Empty);

		using (var f = new StreamWriter(Path.Combine(src_path, base_path, "CharacterSchema.json")))
		{
			f.Write(schema.ToJsonString(Game1.DeserializeOptions));
		}

		#endif
	}
}