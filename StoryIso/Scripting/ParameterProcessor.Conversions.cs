using System;
using StoryIso.Debugging;
using StoryIso.Enums;
using StoryIso.Misc;
using StoryIso.Scripting.Variables;

namespace StoryIso.Scripting;

public partial class ParameterProcessor
{
	public static Optional<T> Convert<T>(Source source, object param) where T : notnull
	{
		return ((FunctionParameter<T>)param).GetValue(source);
	}

	public static Optional<T> Convert<T>(Source source, IFunctionParameter param) where T : notnull
	{
		return ((FunctionParameter<T>)param).GetValue(source);
	}

	public static IOptional ConvertUnknown(Source source, object param, out string? string_value, out Type type)
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
					return default(Optional<string>);
				}

				string_value = optional_string.Value;
				return new Optional<string>(string_value);

			case Optional<float> optional_float:
				type = typeof(float);

				if (!optional_float.HasValue)
				{
					string_value = null;
					return default(Optional<string>);
				}

				string_value = optional_float.Value.ToString();
				return optional_float;

			case Optional<int> optional_int:
				type = typeof(int);

				if (!optional_int.HasValue)
				{	
					string_value = null;
					return default(Optional<string>);
				}

				string_value = optional_int.Value.ToString();

				return optional_int;

			case Optional<bool> optional_bool:
				type = typeof(bool);

				if (!optional_bool.HasValue)
				{
					string_value = null;
					return default(Optional<string>);
				}

				string_value = optional_bool.Value.ToString();
				return optional_bool;

			case FunctionParameter<string> string_param:
				type = typeof(string);
				Optional<string> value = string_param.GetValue(source);

				if (!value.HasValue)
				{
					string_value = null;
					return default(Optional<string>);
				}

				string_value = value.Value;
				return new Optional<string>(string_value);

			case FunctionParameter<float> float_param:
				type = typeof(float);
				Optional<float> float_value = float_param.GetValue(source);

				if (!float_value.HasValue)
				{
					string_value = null;
					return default(Optional<string>);
				}

				string_value = float_value.Value.ToString();
				return float_value;

			case FunctionParameter<int> int_param:
				type = typeof(int);
				Optional<int> int_value = int_param.GetValue(source);

				if (!int_value.HasValue)
				{	
					string_value = null;
					return default(Optional<string>);
				}

				string_value = int_value.Value.ToString();
				if (string_value == null)
				{
					return default(Optional<string>);
				}

				return int_value;

			case FunctionParameter<bool> bool_param:
				type = typeof(bool);
				Optional<bool> bool_value = bool_param.GetValue(source);

				if (!bool_value.HasValue)
				{
					string_value = null;
					return default(Optional<string>);
				}

				string_value = bool_value.Value.ToString();
				return bool_value;

			default:
				throw new NotImplementedException();
		}
	}

	public static IOptional ConvertUnknown(Source source, object param, out string? string_value)
	{
		return ConvertUnknown(source, param, out string_value, out _);
	}

	public static IOptional ConvertUnknown(Source source, object param)
	{
		return ConvertUnknown(source, param, out _, out _);
	}

	public static IOptional? ConvertByType(Source source, IFunctionParameter param)
	{
		var variableType = VariableManager.GetVariableType(param.ValueType);

		return variableType switch
		{
			VariableType.Int => Convert<int>(source, param),
			VariableType.Float => Convert<float>(source, param),
			VariableType.String => Convert<string>(source, param),
			VariableType.Bool => Convert<bool>(source, param),
			_ => throw new NotImplementedException()
		};
	}

	public static Optional<string> ConvertByTypeToString(Source source, IFunctionParameter param)
	{
		var variableType = VariableManager.GetVariableType(param.ValueType);

		return variableType switch
		{
			VariableType.Int => Convert<int>(source, param).ToString() ?? default(Optional<string>),
			VariableType.Float => Convert<float>(source, param).ToString() ?? default(Optional<string>),
			VariableType.String => Convert<string>(source, param).ToString() ?? default(Optional<string>),
			VariableType.Bool => Convert<bool>(source, param).ToString() ?? default(Optional<string>),
			_ => throw new NotImplementedException()
		};
	}

	public static string? ConvertByTypeToString(IOptional param)
	{
        if (!param.HasValue)
        {
	        return null;
        }
        
		var variableType = VariableManager.GetVariableType(param.ValueType);

		return variableType switch
		{
			VariableType.Int => param.ToOptional<int>().ToString(),
			VariableType.Float => param.ToOptional<float>().ToString(),
			VariableType.String => param.ToOptional<string>().ToString(),
			VariableType.Bool => param.ToOptional<bool>().ToString(),
			_ => throw new NotImplementedException()
		};
	}

	public static Optional<string> ConvertByTypeToString(object param, Type type)
	{        
		var variableType = VariableManager.GetVariableType(type);

		switch (variableType)
		{
			case VariableType.Int:
				var int_value = (Optional<int>)param;

				if (!int_value.HasValue)
				{
					return default;
				}

				return int_value.Value.ToString();

			case VariableType.Float:
				var float_value = (Optional<float>)param;

				if (!float_value.HasValue)
				{
					return default;
				}

				return float_value.Value.ToString();

			case VariableType.Bool:
				var bool_value = (Optional<bool>)param;

				if (!bool_value.HasValue)
				{
					return default;
				}

				return bool_value.Value.ToString();

			case VariableType.String:
				var string_value = (Optional<string>)param;

				if (!string_value.HasValue)
				{
					return default;
				}

				return string_value.Value;

			default:
				throw new NotImplementedException();
		}
	}

	public static Optional<RelativeVariable<T>> RelativeConvert<T>(Source source, object param) where T : notnull, IParsable<T>
	{
		var converted_param =  (RelativeVariable<FunctionParameter<T>>)param;

		var value = converted_param.Value.GetValue(source);

		if (!value.HasValue)
		{
			return default;
		}

		return new RelativeVariable<T>(value.Value, converted_param.Relative);
	}

	public static Optional<T[]> ArrayConvert<T>(Source source, object param) where T : notnull
	{
		var array_param = (ArrayParameter<T>)param;

		if (array_param.Length == 0)
		{
			return default;
		}

		return array_param.GetValues(source);
	}

	public static Optional<T> ConvertParam<T>(Source source, IFunctionParameter param) where T : notnull
	{
		if (typeof(T) == param.ValueType)
		{
			return Convert<T>(source, param);
		}

		if (typeof(T) == typeof(string))
		{
			Optional<string> res = ConvertByTypeToString(source, param);

			if (!res.HasValue)
			{
				return default;
			}

			return (Optional<T>)(object)res;
		}

		if (param.ValueType == typeof(float))
		{
			var float_value = Convert<float>(source, param);

			if (typeof(T) == typeof(int))
			{
				return (T)(object)(int)float_value.Value;
			}
		}

		if (param.ValueType == typeof(int))
		{
			var int_value = Convert<int>(source, param);

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

	public static IOptional? ConvertParam(Source source, IFunctionParameter param, Type return_type)
	{
		if (return_type == param.ValueType || return_type == typeof(VariableObject))
		{
			return ConvertByType(source, param) ?? null;
		}

		if (return_type == typeof(string))
		{
			Optional<string> res = ConvertByTypeToString(source, param);

			if (!res.HasValue)
			{
				return null;
			}

			return res;
		}

		if (param.ValueType == typeof(float))
		{
			var float_value = Convert<float>(source, param);

			if (return_type == typeof(int))
			{
				if (!float_value.HasValue)
				{
					return new Optional<int>();
				}
                
				return new Optional<int>((int)float_value.Value);
			}
		}

		if (param.ValueType == typeof(int))
		{
			var int_value = Convert<int>(source, param);

			if (return_type == typeof(float))
			{
				if (!int_value.HasValue)
				{
					return new Optional<float>();
				}
                
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

	public static Optional<T> ConvertOptional<T>(Source source, IOptional param) where T : notnull
	{
		if (!param.HasValue)
		{
			return default;
		}

		if (typeof(T) == param.ValueType)
		{
			return (Optional<T>)param;
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

		if (param.ValueType == typeof(string))
		{
			var string_value = (Optional<string>)param;

			if (typeof(T) == typeof(float) && float.TryParse(string_value.Value, out var parsed_float))
			{
				return new Optional<T>((T)(object)parsed_float);
			}

			if (typeof(T) == typeof(int) && int.TryParse(string_value.Value, out var parsed_int))
			{
				return new Optional<T>((T)(object)parsed_int);
			}

			if (typeof(T) == typeof(bool) && bool.TryParse(string_value.Value, out var parsed_bool))
			{
				return new Optional<T>((T)(object)parsed_bool);
			}

			DebugConsole.Raise(new ParameterTypeError(source, "Convert", string_value.Value, typeof(T).Name));
			return default;
		}

		if (param.ValueType == typeof(float))
		{
			var float_value = (Optional<float>)param;

			if (typeof(T) == typeof(int))
			{
				return (T)(object)(int)float_value.Value;
			}

			DebugConsole.Raise(new ParameterTypeError(source, "Convert", float_value.Value.ToString(), typeof(T).Name));
			return default;
		}

		if (param.ValueType == typeof(int))
		{
			var int_value = (Optional<int>)param;

			if (typeof(T) == typeof(float))
			{
				return (T)(object)(float)int_value.Value;
			}

			DebugConsole.Raise(new ParameterTypeError(source, "Convert", int_value.Value.ToString(), typeof(T).Name));
			return default;
		}

		if (param.ValueType == typeof(bool))
		{
			var bool_value = (Optional<bool>)param;
			// add conversions?

			DebugConsole.Raise(new ParameterTypeError(source, "Convert", bool_value.Value.ToString(), typeof(T).Name));
			return default;
		}

		throw new NotImplementedException();
	}

	public static IOptional ConvertOptional(Source source, IOptional param, VariableType type)
	{
		var param_type = VariableManager.GetVariableType(param.ValueType);

		if (type == param_type)
		{
			return param;
		}

		if (type == VariableType.String)
		{
			var res = param.ToString();

			if (res == null)
			{
				return new Optional<string>();
			}

			return new Optional<string>(res);
		}

		switch (param_type)
		{
			case VariableType.String:
				var string_value = (Optional<string>)param;

				Optional<T> try_parse<T>() where T : IParsable<T>
				{
					if (!T.TryParse(string_value.Value, null, out var parsed_value))
					{
						DebugConsole.Raise(new ParameterTypeError(source, "Convert", string_value.Value, typeof(T).Name));
						return default;
					} 

					return new Optional<T>(parsed_value);
				}

				return type switch
				{
					VariableType.Float => try_parse<float>(),
					VariableType.Int => try_parse<int>(),
					VariableType.Bool => try_parse<bool>(),
					_ => throw new NotImplementedException()
				};

			case VariableType.Float:
				var float_value = (Optional<float>)param;

				if (type == VariableType.Int)
				{
					return new Optional<int>((int)float_value.Value);
				}

				break;

			case VariableType.Int:
				var int_value = (Optional<int>)param;

				if (type == VariableType.Float)
				{
					return new Optional<float>(int_value.Value);
				}

				break;

			case VariableType.Bool:
				break;

		}

		DebugConsole.Raise(new ParameterTypeError(source, "Convert", param.ToString() ?? "null", VariableManager.GetVariableTypeName(type)));
		return new Optional<string>();
	}
}