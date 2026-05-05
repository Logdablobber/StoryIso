using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Schema;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Entropy.Debugging;
using Entropy.Enums;
using Entropy.Scenes;

namespace Entropy.Input;

public class InputProcessor
{
	public Dictionary<KeyEvent, string[]> Keybinds = [];

	private KeyboardState? _previousKeystate;

	public InputProcessor(string base_path)
	{
		using var streamReader = new StreamReader(base_path + "Keybinds.json");

		string json_text = streamReader.ReadToEnd();

		var keybinds_data = JsonSerializer.Deserialize<KeybindsData>(json_text, Game1.DeserializeOptions);

		foreach (var keybind in keybinds_data.Keybinds)
		{
			if (keybind.KeyDown != null)
			{
				var new_event = new KeyEvent(keybind.Key, KeyInteraction.Down);

				Keybinds.Add(new_event, keybind.KeyDown);
			}
            
            if (keybind.KeyUp != null)
            {
	            var new_event = new KeyEvent(keybind.Key, KeyInteraction.Up);

	            Keybinds.Add(new_event, keybind.KeyUp);
            }
            
            if (keybind.KeyHeld != null)
            {
	            var new_event = new KeyEvent(keybind.Key, KeyInteraction.Hold);

	            Keybinds.Add(new_event, keybind.KeyHeld);
            }
		}

#if DEBUG

		var schema = Game1.DeserializeOptions.GetJsonSchemaAsNode(typeof(KeybindsData));

		var path = Path.GetFullPath("./");

		var src_path = Regex.Replace(path, "bin.+", string.Empty);

		using (var f = new StreamWriter(Path.Combine(src_path, base_path, "KeybindsSchema.json")))
		{
			f.Write(schema.ToJsonString(Game1.DeserializeOptions));
		}

#endif
	}

	public void Process(KeyboardState keystate)
	{
		var source = new Source(0, null, "KeybindManager");

		foreach (var (keyEvent, scenes) in Keybinds)
		{
			if (!CheckRunKeybind(keystate, keyEvent))
			{
				continue;
			}

			foreach (var scene in scenes)
			{
				Game1.sceneManager.RunScene(scene, source, true);
			}
		}

		_previousKeystate = keystate;
	}

	private bool CheckRunKeybind(KeyboardState keystate, KeyEvent keyEvent)
	{
		switch (keyEvent.Interaction)
		{
			case KeyInteraction.Down:
				if (!keystate.IsKeyDown(keyEvent.Key) ||
				    (_previousKeystate != null && !_previousKeystate.Value.IsKeyUp(keyEvent.Key)))
				{
					return false;
				}

				return true;

			case KeyInteraction.Up:
				if (!keystate.IsKeyUp(keyEvent.Key) ||
				    (_previousKeystate != null && !_previousKeystate.Value.IsKeyDown(keyEvent.Key)))
				{
					return false;
				}

				return true;

			case KeyInteraction.Hold:
				if (keystate.IsKeyUp(keyEvent.Key))
				{
					return false;
				}

				return true;

			default:
				throw new NotImplementedException();
		}
	}
}