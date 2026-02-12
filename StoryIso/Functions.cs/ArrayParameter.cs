using System.Collections.Generic;

namespace StoryIso.Functions;

public struct ArrayParameter<T> where T : notnull
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

	public readonly T[]? GetValues()
	{
		T[] res = new T[values.Length];

		for (int i = 0; i < values.Length; i++)
		{
			T? value = values[i].Value;

			if (value == null)
			{
				return null;
			}

			res[i] = value;
		} 

		return res;
	}

	public readonly T? Get(int index)
	{
		return values[index].Value;
	}
}