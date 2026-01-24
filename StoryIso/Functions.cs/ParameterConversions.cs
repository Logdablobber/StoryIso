using System;
using System.Collections.Generic;
using StoryIso.Misc;

namespace StoryIso.Functions;

public partial class FunctionProcessor
{
	public static T Convert<T>(object param)
	{
		return (T)(FunctionParameter<T>)param;
	}

	public static RelativeVariable<T>? RelativeConvert<T>(object param) where T : struct
	{
		var converted_param = (RelativeVariable<FunctionParameter<T?>>)param;

		var value = converted_param.Value.Value;

		if (!value.HasValue)
		{
			return null;
		}

		return new RelativeVariable<T>(converted_param.Value.Value.Value, converted_param.Relative);
	}

	public static T[] ArrayConvert<T>(object param)
	{
		var array_param = (ArrayParameter<T>)param;

		if (array_param.Length == 0)
		{
			return null;
		}

		T[] res = new T[array_param.Length];

		for (int i = 0; i < array_param.Length; i++)
		{
			T value = array_param.Get(i);

			if (value == null)
			{
				return null;
			}

			res[i] = value;
		}

		return res;
	}
}