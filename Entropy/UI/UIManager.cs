using System.Collections.Generic;
using System.IO;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.ECS;

namespace Entropy.UI;

public static class UIManager
{
	private readonly static Dictionary<string, UIObject> _UIObjects = [];

	public static readonly List<string> UIElements = [];

	public static void SetObjectPosition(string name, Vector2 position)
	{
		if (!_UIObjects.TryGetValue(name, out UIObject? obj))
		{
			return;
		}

		obj.UpdatePosition(position);
	}

	public static void SetObjectX(string name, float x)
	{
		if (!_UIObjects.TryGetValue(name, out UIObject? obj))
		{
			return;
		}

		obj.UpdateX(x);
	}

	public static void SetObjectY(string name, float y)
	{
		if (!_UIObjects.TryGetValue(name, out UIObject? obj))
		{
			return;
		}

		obj.UpdateY(y);
	}

	public static void SetObjectVisible(string name, bool visible)
	{
		if (!_UIObjects.TryGetValue(name, out UIObject? obj))
		{
			return;
		}

		obj.info.Visible = visible;
	}

	public static void SetObjectScale(string name, float scale)
	{
		if (!_UIObjects.TryGetValue(name, out UIObject? obj))
		{
			return;
		}

		obj.info.Scale = new Vector2(scale);
	}

	private static void AddObject(UIObject obj)
	{
		_UIObjects.Add(obj.info.Name, obj);
        
        UIElements.Add(obj.info.Name);
        UIElements.AddRange(obj.Parts);

		foreach (var child in obj.Children)
		{
			AddObject(child);
		}
	}

	public static void LoadAll(GraphicsDevice graphics, string base_path, World world)
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

			var ui_object = new UIObject(null, graphics, ui_data.Value, world);

			AddObject(ui_object);
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