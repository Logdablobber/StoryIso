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

	public static Scene? ProcessScene(string obj, string[] lines)
	{
		if (!lines[0].StartsWith('#'))
		{
			DebugConsole.Raise(new InvalidSceneError(new Source(1, null, obj), "Scene should start with '#NAME' where 'NAME' is the name of the scene."));

			return null;	
		}

		string name = lines[0][1..];

		Scope? scope = FunctionProcessor.Process(obj, lines, 1);

		if (scope == null)
		{
			return null;
		}

		return new Scene(name, scope);
	}
}