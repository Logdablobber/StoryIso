using System;
using System.Collections.Generic;
using StoryIso.Debugging;
using StoryIso.Enums;

namespace StoryIso.Functions;

public class FunctionDef
{
	public string? name;
	public FunctionType type;
	public Type[]? parameters;
	public Func<List<object>?, Source?, uint?>? function;
}