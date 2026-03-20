using System.Collections.Generic;
using System.IO;
using StoryIso.Debugging;
using StoryIso.Scripting;
using StoryIso.Misc;

namespace StoryIso.Scenes;

public static class SceneProcessor
{
	public static Scene? ProcessScene(string path)
	{
		string scene_text;
		using(StreamReader streamReader = new StreamReader(path))
		{
			scene_text = streamReader.ReadToEnd();
		}

		string[] scene_lines = TextFormatter.SplitLines(scene_text);

		if (!scene_lines[0].StartsWith('#'))
		{
			DebugConsole.Raise(new InvalidSceneError(new Source(1, null, path), "Scene should start with '#NAME' where 'NAME' is the name of the scene."));	

			return null;	
		}

		string name = scene_lines[0][1..];

		Scope? scope = FunctionProcessor.Process(path, scene_text, 1);

		if (scope == null)
		{
			return null;
		}

		return new Scene(name, scope);
	}
}