using System.Collections.Generic;

namespace StoryIso.Functions;

public struct Function
{
	public ushort functionIndex;
	public List<object> parameters;
	public uint line;

	public Function(ushort function_index, List<object> parameters, uint line) : this()
	{
		this.functionIndex = function_index;
		this.parameters = parameters;
		this.line = line;
	}
}