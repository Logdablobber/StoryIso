using System.Collections.Generic;
using StoryIso.Scripting;

namespace StoryIso.Scenes;

public class Scene
{
	public readonly string name;
	public List<Function> functions;

	public Scene(string name, List<Function> functions)
	{
		this.name = name;
		this.functions = functions;
	}
}