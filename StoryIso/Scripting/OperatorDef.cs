using System;
using System.Collections.Generic;
using StoryIso.Debugging;
using StoryIso.Misc;

namespace StoryIso.Scripting;

public class OperatorDef
{
	public required string oper;
	public bool isConstant;
	public bool inlineFunc;
	public bool sync;
	public required Type[] parameters;
	public required Type returnType;
	public Func<List<IOptional>, Source, IOptional?>? function;
}