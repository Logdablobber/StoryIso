using System;
using System.Collections.Generic;
using StoryIso.Misc;

namespace StoryIso.Functions;

public partial class FunctionProcessor
{
	public static T? Convert<T>(object param) where T : notnull
	{
		return ((FunctionParameter<T>)param).Value;
	}

	public static object? ConvertUnknown(object param, out string? string_value)
	{
		Type type = param.GetType();

		if (type == typeof(FunctionParameter<string>))
		{
			string_value = Convert<string>(param);
			return string_value;
		}

		if (type == typeof(FunctionParameter<float>))
		{
			float? value = Convert<float>(param);

			if (!value.HasValue)
			{
				string_value = null;
				return null;
			}

			string_value = value.ToString();
			return value;
		}

		if (type == typeof(FunctionParameter<int>))
		{
			int? value = Convert<int>(param);

			if (!value.HasValue)
			{	
				string_value = null;
				return null;
			}

			string_value = value.ToString();
			return value.ToString();
		}

		if (type == typeof(FunctionParameter<bool>))
		{
			bool? value = Convert<bool>(param);

			if (!value.HasValue)
			{
				string_value = null;
				return null;
			}

			string_value = value.ToString();
			return value;
		}

		string_value = null;
		return null;
	}

	public static object? ConvertUnknown(object param)
	{
		return ConvertUnknown(param, out _);
	}

	public static RelativeVariable<T>? RelativeConvert<T>(object param) where T : notnull
	{
		var converted_param = (RelativeVariable<FunctionParameter<T>>)param;

		var value = converted_param.Value.Value;

		if (value == null)
		{
			return null;
		}

		return new RelativeVariable<T>(value, converted_param.Relative);
	}

	public static T[]? ArrayConvert<T>(object param) where T : notnull
	{
		var array_param = (ArrayParameter<T>)param;

		if (array_param.Length == 0)
		{
			return null;
		}

		return array_param.GetValues();
	}
}