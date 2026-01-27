using System;
using System.Collections.Generic;
using StoryIso.Debugging;
using StoryIso.Enums;

namespace StoryIso.Functions;

public struct OperatorDef
{
	public string oper;
	public Type[] parameters;
	public Type returnType;
	public Func<List<object>, Source, object> function;
}