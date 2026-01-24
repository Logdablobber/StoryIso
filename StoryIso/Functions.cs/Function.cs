using System.Collections.Generic;
using StoryIso.Enums;

namespace StoryIso.Functions;

public struct Function
{
	public FunctionType function;
	public List<object> parameters;
	public uint line;

	public Function(FunctionType function, List<object> parameters, uint line) : this()
	{
		this.function = function;
		this.parameters = parameters;
		this.line = line;
	}
}