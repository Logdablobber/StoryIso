using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.RegularExpressions;
using MonoGame.Extended.ECS;

namespace StoryIso.UI;

public static class UIManager
{
	private readonly static List<UIObject> _UIObjects = [];

	public static void LoadAll(string base_path, World world)
	{
		_UIObjects.Clear();

		var dir = new DirectoryInfo(base_path);

		var files = dir.GetFiles("*.json");

		foreach (var f in files)
		{
			if (f.Name == "UISchema.json")
			{
				continue;
			}

			string json_text;
			using (var reader = new StreamReader(f.FullName))
			{
				json_text = reader.ReadToEnd();
			}

			UIData? ui_data = UIGenerator.GenerateUI(json_text);

			if (ui_data == null)
			{
				continue;
			}

			var ui_object = new UIObject(ui_data.Value, world);

			_UIObjects.Add(ui_object);
		}

		#if DEBUG

		JsonNode schema = Game1.DeserializeOptions.GetJsonSchemaAsNode(typeof(UIData));

		var path = Path.GetFullPath("./");

		var src_path = Regex.Replace(path, @"bin.+", string.Empty);

		using (var f = new StreamWriter(Path.Combine(src_path, base_path, "UISchema.json")))
		{
			f.Write(schema.ToJsonString(Game1.DeserializeOptions));
		}

		#endif
	}
}