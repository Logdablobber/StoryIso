using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Input;
using Entropy.Debugging;
using Entropy.Input;
using Entropy.Scripting;
using Entropy.Misc;
using Entropy.Enums;

namespace Entropy.Scenes;

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
        
        // TODO: make scenes able to take in arguments

		string name = lines[0][1..];

		Scope? scope = FunctionProcessor.Process(obj, lines, 1);

		if (scope == null)
		{
			return null;
		}

		return new Scene(name, scope);
	}
}