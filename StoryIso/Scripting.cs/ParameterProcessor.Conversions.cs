using System;
using StoryIso.Enums;
using StoryIso.Misc;

namespace StoryIso.Scripting;

public partial class ParameterProcessor
{
	public static Optional<T> Convert<T>(object param) where T : notnull
	{
		return ((FunctionParameter<T>)param).Value;
	}

	public static Optional<T> Convert<T>(IFunctionParameter param) where T : notnull
	{
		return ((FunctionParameter<T>)param).Value;
	}

	public static object? ConvertUnknown(object param, out string? string_value, out Type type)
	{
		string_value = null;
		type = null!;

		switch (param)
		{
			case Optional<string> optional_string:
				type = typeof(string);

				if (!optional_string.HasValue)
				{
					string_value = null;
					return null;
				}

				string_value = optional_string.Value;
				return new Optional<string>(string_value);

			case Optional<float> optional_float:
				type = typeof(float);

				if (!optional_float.HasValue)
				{
					string_value = null;
					return null;
				}

				string_value = optional_float.Value.ToString();
				return optional_float;

			case Optional<int> optional_int:
				type = typeof(int);

				if (!optional_int.HasValue)
				{	
					string_value = null;
					return null;
				}

				string_value = optional_int.Value.ToString();

				return optional_int;

			case Optional<bool> optional_bool:
				type = typeof(bool);

				if (!optional_bool.HasValue)
				{
					string_value = null;
					return null;
				}

				string_value = optional_bool.Value.ToString();
				return optional_bool;

			case FunctionParameter<string> string_param:
				type = typeof(string);
				Optional<string> value = string_param.Value;

				if (!value.HasValue)
				{
					string_value = null;
					return null;
				}

				string_value = value.Value;
				return new Optional<string>(string_value);

			case FunctionParameter<float> float_param:
				type = typeof(float);
				Optional<float> float_value = float_param.Value;

				if (!float_value.HasValue)
				{
					string_value = null;
					return null;
				}

				string_value = float_value.Value.ToString();
				return float_value;

			case FunctionParameter<int> int_param:
				type = typeof(int);
				Optional<int> int_value = int_param.Value;

				if (!int_value.HasValue)
				{	
					string_value = null;
					return null;
				}

				string_value = int_value.Value.ToString();
				if (string_value == null)
				{
					return null;
				}

				return int_value;

			case FunctionParameter<bool> bool_param:
				type = typeof(bool);
				Optional<bool> bool_value = bool_param.Value;

				if (!bool_value.HasValue)
				{
					string_value = null;
					return null;
				}

				string_value = bool_value.Value.ToString();
				return bool_value;

			default:
				throw new NotImplementedException();
		}
	}

	public static object? ConvertUnknown(object param, out string? string_value)
	{
		return ConvertUnknown(param, out string_value, out _);
	}

	public static object? ConvertUnknown(object param)
	{
		return ConvertUnknown(param, out _, out _);
	}

	public static IOptional? ConvertByType(IFunctionParameter param)
	{
		var variableType = VariableManager.GetVariableType(param.ValueType);

		return variableType switch
		{
			VariableType.Int => Convert<int>(param),
			VariableType.Float => Convert<float>(param),
			VariableType.String => Convert<string>(param),
			VariableType.Bool => Convert<bool>(param),
			_ => throw new NotImplementedException()
		};
	}

	public static string? ConvertByTypeToString(IFunctionParameter param)
	{
		var variableType = VariableManager.GetVariableType(param.ValueType);

		return variableType switch
		{
			VariableType.Int => Convert<int>(param).ToString(),
			VariableType.Float => Convert<float>(param).ToString(),
			VariableType.String => Convert<string>(param).ToString(),
			VariableType.Bool => Convert<bool>(param).ToString(),
			_ => throw new NotImplementedException()
		};
	}

	public static string? ConvertByTypeToString(IOptional param)
	{
		var variableType = VariableManager.GetVariableType(param.ValueType);

		return variableType switch
		{
			VariableType.Int => ((Optional<int>)param).ToString(),
			VariableType.Float => ((Optional<float>)param).ToString(),
			VariableType.String => ((Optional<string>)param).ToString(),
			VariableType.Bool => ((Optional<bool>)param).ToString(),
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

	public static Optional<T> ConvertParam<T>(IFunctionParameter param) where T : notnull
	{
		if (typeof(T) == param.ValueType)
		{
			return Convert<T>(param);
		}

		if (typeof(T) == typeof(string))
		{
			string? res = ConvertByTypeToString(param);

			if (res == null)
			{
				return default;
			}

			return (T)(object)res;
		}

		if (param.ValueType == typeof(float))
		{
			var float_value = Convert<float>(param);

			if (typeof(T) == typeof(int))
			{
				return (T)(object)(int)float_value.Value;
			}
		}

		if (param.ValueType == typeof(int))
		{
			var int_value = Convert<int>(param);

			if (typeof(T) == typeof(float))
			{
				return (T)(object)(float)int_value.Value;
			}
		}

		if (param.ValueType == typeof(bool))
		{
			//var bool_value = Convert<bool>(param);
			// add conversions?
		}

		throw new NotImplementedException();
	}

	public static IOptional? ConvertParam(IFunctionParameter param, Type return_type)
	{
		if (return_type == param.ValueType || return_type == typeof(VariableObject))
		{
			return ConvertByType(param) ?? null;
		}

		if (return_type == typeof(string))
		{
			string? res = ConvertByTypeToString(param);

			if (res == null)
			{
				return null;
			}

			return new Optional<string>(res);
		}

		if (param.ValueType == typeof(float))
		{
			var float_value = Convert<float>(param);

			if (return_type == typeof(int))
			{
				return new Optional<int>((int)float_value.Value);
			}
		}

		if (param.ValueType == typeof(int))
		{
			var int_value = Convert<int>(param);

			if (return_type == typeof(float))
			{
				return new Optional<float>(int_value.Value);
			}
		}

		if (param.ValueType == typeof(bool))
		{
			//var bool_value = Convert<bool>(param);
			// add conversions?
		}

		throw new NotImplementedException();
	}

	public static Optional<T> ConvertOptional<T>(IOptional param) where T : notnull
	{
		if (typeof(T) == param.ValueType)
		{
			return (Optional<T>)(object)param;
		}

		if (typeof(T) == typeof(string))
		{
			string? res = param.ToString();

			if (res == null)
			{
				return default;
			}

			return (T)(object)res;
		}

		if (param.ValueType == typeof(float))
		{
			var float_value = (Optional<float>)param;

			if (typeof(T) == typeof(int))
			{
				return (T)(object)(int)float_value.Value;
			}
		}

		if (param.ValueType == typeof(int))
		{
			var int_value = (Optional<int>)param;

			if (typeof(T) == typeof(float))
			{
				return (T)(object)(float)int_value.Value;
			}
		}

		if (param.ValueType == typeof(bool))
		{
			//var bool_value = Convert<bool>(param);
			// add conversions?
		}

		throw new NotImplementedException();
	}
}