using System;
using System.Collections.Generic;
using Entropy.Debugging;
using Entropy.Misc;

namespace Entropy.Scripting;

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