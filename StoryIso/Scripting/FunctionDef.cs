using System;
using System.Collections.Generic;
using StoryIso.Debugging;
using StoryIso.Misc;
using StoryIso.Scripting.Variables;

namespace StoryIso.Scripting;

public class FunctionDef
{
	public string? name;
	public required Type[] parameters;
	public Func<ScopeVariables, IOptional[], Source, uint?>? function;
}