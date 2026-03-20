using System;
using System.Collections.Generic;
using StoryIso.Debugging;
using StoryIso.Scripting.Variables;

namespace StoryIso.Scripting;

public class FunctionDef
{
	public string? name;
	public Type[]? parameters;
	public Func<ScopeVariables, List<object>?, Source, uint?>? function;
}