using System.Collections.Generic;
using StoryIso.Scripting;

namespace StoryIso.Scenes;

public class Scene
{
	public readonly string name;
	public Scope scope;

	public Scene(string name, Scope scope)
	{
		this.name = name;
		this.scope = scope;
	}
}