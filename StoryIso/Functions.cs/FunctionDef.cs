using System;
using System.Collections.Generic;
using StoryIso.Debugging;

namespace StoryIso.Functions;

public class FunctionDef
{
	public string? name;
	public Type[]? parameters;
	public Func<List<object>?, Source?, uint?>? function;
}