using System;
using System.Collections.Generic;
using Entropy.Debugging;
using Entropy.Misc;
using Entropy.Scripting.Variables;

namespace Entropy.Scripting;

public class FunctionDef
{
	public string? name;
	public required Type[] parameters;
	public Func<ScopeVariables, IOptional[], Source, uint?>? function;
}