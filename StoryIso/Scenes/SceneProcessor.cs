using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Input;
using StoryIso.Debugging;
using StoryIso.Input;
using StoryIso.Scripting;
using StoryIso.Misc;
using StoryIso.Enums;

namespace StoryIso.Scenes;

public static class SceneProcessor
{
	public static void PreprocessScene(string obj, string[] lines)
	{
		if (!lines[0].StartsWith('#'))
		{
			DebugConsole.Raise(new InvalidSceneError(new Source(1, null, obj), "Scene should start with '#NAME' where 'NAME' is the name of the scene."));	

			return;	
		}

		FunctionProcessor.Preprocess(obj, lines, 1);
	}

	public static Scene? ProcessScene(string obj, string[] lines, out Optional<KeyEvent> bind_to_key)
	{
		if (!lines[0].StartsWith('#'))
		{
			DebugConsole.Raise(new InvalidSceneError(new Source(1, null, obj), "Scene should start with '#NAME' where 'NAME' is the name of the scene."));

			bind_to_key = default;
			return null;	
		}

		string name = lines[0][1..];

		uint start_line = 1;
        
        if (lines.Length > 1 && lines[1].StartsWith("#BindTo"))
        {            
	        var words = lines[1].Split(' ');
            
            if (words.Length != 3)
			{
				DebugConsole.Raise(new InvalidSceneError(new Source(2, null, obj),
					"Keybinding should be in the format '#BindTo KEY ACTION'"));
			}
            
	        if (!Enum.TryParse(words[1], true, out Keys key))
	        {
		        DebugConsole.Raise(new InvalidSceneError(new Source(2, null, obj), $"Unknown key '{words[1]}'"));
		        bind_to_key = default;
		        return null;
	        }
            
            if (!Enum.TryParse(words[2], true, out KeyInteraction interaction) || interaction == KeyInteraction.None)
            {
	            DebugConsole.Raise(new InvalidSceneError(new Source(2, null, obj), $"Unknown interaction '{words[2]}'"));
	            bind_to_key = default;
	            return null;
            }

            bind_to_key = new KeyEvent
            {
	            Key = key,
	            Interaction = interaction
            };

            start_line += 1;
        }
        else
        {
	        bind_to_key = default;
        }

		Scope? scope = FunctionProcessor.Process(obj, lines, start_line);

		if (scope == null)
		{
			return null;
		}

		return new Scene(name, scope);
	}
}