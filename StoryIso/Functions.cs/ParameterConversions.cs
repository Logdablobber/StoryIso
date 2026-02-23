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
		type = param.GetType();

		if (type == typeof(FunctionParameter<string>))
		{
			var value = Convert<string>(param);

			if (!value.HasValue)
			{
				string_value = null;
				return null;
			}

			string_value = value.Value;
			return new Optional<string>(string_value);
		}

		if (type == typeof(FunctionParameter<float>))
		{
			Optional<float> value = Convert<float>(param);

			if (!value.HasValue)
			{
				string_value = null;
				return null;
			}

			string_value = value.Value.ToString();
			return value;
		}

		if (type == typeof(FunctionParameter<int>))
		{
			Optional<int> value = Convert<int>(param);

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

		if (type == typeof(FunctionParameter<bool>))
		{
			Optional<bool> value = Convert<bool>(param);

			if (!value.HasValue)
			{
				string_value = null;
				return null;
			}

			string_value = value.Value.ToString();
			return value;
		}

		if (type == typeof(Optional<string>))
		{
			var value = (Optional<string>)param;

			if (!value.HasValue)
			{
				string_value = null;
				return null;
			}

			string_value = value.Value;
			return new Optional<string>(string_value);
		}

		if (type == typeof(Optional<float>))
		{
			var value = (Optional<float>)param;

			if (!value.HasValue)
			{
				string_value = null;
				return null;
			}

			string_value = value.Value.ToString();
			return value;
		}

		if (type == typeof(Optional<int>))
		{
			var value = (Optional<int>)param;

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

		if (type == typeof(Optional<bool>))
		{
			var value = (Optional<bool>)param;

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