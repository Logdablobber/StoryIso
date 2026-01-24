using System.Collections.Generic;

namespace StoryIso.Functions;

public struct ArrayParameter<T>
{
	private readonly FunctionParameter<T>[] values;

	public readonly int Length { get { return values.Length; } }

	public ArrayParameter(FunctionParameter<T>[] values)
	{
		this.values = values;
	}

	public ArrayParameter(List<FunctionParameter<T>> values)
	{
		this.values = values.ToArray();
	}

	public readonly T Get(int index)
	{
		return (T)values[index];
	}
}