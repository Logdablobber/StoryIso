using System.Collections.Generic;

namespace StoryIso.Scripting;

public struct Function : IScriptObject
{
	public ushort functionIndex;
	public List<object> parameters;
	private readonly uint _line;
	public uint Line
	{
		get
		{
			return _line;
		}
	}

	public bool IsScope
	{
		get
		{
			return false;
		}
	}

	public Function(ushort function_index, List<object> parameters, uint line) : this()
	{
		this.functionIndex = function_index;
		this.parameters = parameters;
		this._line = line;
	}
}