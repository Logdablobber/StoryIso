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

	public static string ConvertBase(object param)
	{
		Type type = param.GetType();

		if (type == typeof(FunctionParameter<string>))
		{
			return Convert<string>(param);
		}

		if (type == typeof(FunctionParameter<float?>))
		{
			float? value = Convert<float?>(param).Value;

			if (!value.HasValue)
			{
				return null;
			}

			return value.ToString();
		}

		if (type == typeof(FunctionParameter<int?>))
		{
			int? value = Convert<int?>(param).Value;

			if (!value.HasValue)
			{
				return null;
			}

			return value.ToString();
		}

		if (type == typeof(FunctionParameter<bool?>))
		{
			bool? value = Convert<bool?>(param).Value;

			if (!value.HasValue)
			{
				return null;
			}

			return value.ToString();
		}

		return null;
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