using System.Collections.Generic;
using Entropy.Debugging;
using Entropy.Misc;

namespace Entropy.Scripting;

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

	public readonly Optional<T[]> GetValues(Source source)
	{
		T[] res = new T[values.Length];

		for (int i = 0; i < values.Length; i++)
		{
			Optional<T> value = values[i].GetValue(source);

			if (!value.HasValue)
			{
				return default;
			}

			res[i] = value.Value;
		} 

		return res;
	}

	public readonly Optional<T> Get(Source source, int index)
	{
		return values[index].GetValue(source);
	}
}