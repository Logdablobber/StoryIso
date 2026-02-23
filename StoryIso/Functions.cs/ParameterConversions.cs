using System;
using System.Collections.Generic;
using System.Data;
using StoryIso.Enums;
using StoryIso.Misc;

namespace StoryIso.Functions;

public partial class FunctionProcessor
{
	public static Optional<T> Convert<T>(object param) where T : notnull
	{
		return ((FunctionParameter<T>)param).Value;
	}

	public static object? ConvertUnknown(object param, out string? string_value, out Type type)
	{
		if (param is Optional<string> optional_string)
		{
			type = typeof(string);

			if (!optional_string.HasValue)
			{
				string_value = null;
				return null;
			}

			string_value = optional_string.Value;
			return new Optional<string>(string_value);
		}

		if (param is Optional<float> optional_float)
		{
			type = typeof(float);

			if (!optional_float.HasValue)
			{
				string_value = null;
				return null;
			}

			string_value = optional_float.Value.ToString();
			return optional_float;
		}

		if (param is Optional<int> optional_int)
		{
			type = typeof(int);

			if (!optional_int.HasValue)
			{	
				string_value = null;
				return null;
			}

			string_value = optional_int.Value.ToString();

			return optional_int;
		}

		if (param is Optional<bool> optional_bool)
		{
			type = typeof(bool);

			if (!optional_bool.HasValue)
			{
				string_value = null;
				return null;
			}

			string_value = optional_bool.Value.ToString();
			return optional_bool;
		}

		if (param is FunctionParameter<string> string_param)
		{
			type = typeof(string);
			var value = string_param.Value;

			if (!value.HasValue)
			{
				string_value = null;
				return null;
			}

			string_value = value.Value;
			return new Optional<string>(string_value);
		}

		if (param is FunctionParameter<float> float_param)
		{	
			type = typeof(float);
			Optional<float> value = float_param.Value;

			if (!value.HasValue)
			{
				string_value = null;
				return null;
			}

			string_value = value.Value.ToString();
			return value;
		}

		if (param is FunctionParameter<int> int_param)
		{
			type = typeof(int);
			Optional<int> value = int_param.Value;

			if (!value.HasValue)
			{	
				string_value = null;
				return null;
			}

			string_value = value.Value.ToString();
			if (string_value == null)
			{
				return null;
			}

			return value;
		}

		if (param is FunctionParameter<bool> bool_param)
		{
			type = typeof(bool);
			Optional<bool> value = bool_param.Value;

			if (!value.HasValue)
			{
				string_value = null;
				return null;
			}

			string_value = value.Value.ToString();
			return value;
		}

		throw new NotImplementedException();
	}

	public static object? ConvertUnknown(object param, out string? string_value)
	{
		return ConvertUnknown(param, out string_value, out _);
	}

	public static object? ConvertUnknown(object param)
	{
		return ConvertUnknown(param, out _, out _);
	}

	public static object? ConvertByType(object param, Type type)
	{
		var variableType = VariableManager.GetVariableType(type);

		return variableType switch
		{
			VariableType.Int => Convert<int>(param),
			VariableType.Float => Convert<float>(param),
			VariableType.String => Convert<string>(param),
			VariableType.Bool => Convert<bool>(param),
			_ => throw new NotImplementedException()
		};
	}

	public static string? ConvertByTypeToString(object param, Type type)
	{
		var variableType = VariableManager.GetVariableType(type);

		switch (variableType)
		{
			case VariableType.Int:
				var int_value = (Optional<int>)param;

				if (!int_value.HasValue)
				{
					return null;
				}

				return int_value.Value.ToString();

			case VariableType.Float:
				var float_value = (Optional<float>)param;

				if (!float_value.HasValue)
				{
					return null;
				}

				return float_value.Value.ToString();

			case VariableType.Bool:
				var bool_value = (Optional<bool>)param;

				if (!bool_value.HasValue)
				{
					return null;
				}

				return bool_value.Value.ToString();

			case VariableType.String:
				var string_value = (Optional<string>)param;

				if (!string_value.HasValue)
				{
					return null;
				}

				return string_value.Value;

			default:
				throw new NotImplementedException();
		}
	}

	public static RelativeVariable<T>? RelativeConvert<T>(object param) where T : notnull, IParsable<T>
	{
		var converted_param =  (RelativeVariable<FunctionParameter<T>>)param;

		var value = converted_param.Value.Value;

		if (!value.HasValue)
		{
			return null;
		}

		return new RelativeVariable<T>(value.Value, converted_param.Relative);
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