using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using StoryIso.Debugging;
using StoryIso.Enums;
using StoryIso.Misc;

namespace StoryIso.Functions;

public static partial class ParameterProcessor
{
	private static TileLayerType GetLayerType(string name)
	{
		return name.ToLower() switch
		{
			"floorlayer" => TileLayerType.FloorLayer,
			"walllayer" => TileLayerType.WallLayer,
			"interactiontilelayer" => TileLayerType.InteractionLayer,
			_ => TileLayerType.None,
		};
	}

	public static Direction GetDirection(string name)
	{
		return name.ToLower() switch
		{
			"up" => Direction.Up,
			"left" => Direction.Left,
			"down" => Direction.Down,
			"right" => Direction.Right,
			_ => Direction.None,
		};
	}
	public static FunctionParameter<T>? ParseParameter<T>(string value, Source source, string function) where T : notnull, IParsable<T>
	{
		if (!T.TryParse(value, null, out T? parsed_value))
		{
			DebugConsole.Raise(new ParameterTypeError(source, function, value, typeof(T).FullName ?? "Type.. doesn't have a name?"));
			return null;
		}

		return new FunctionParameter<T>(parsed_value);
	}

	public static FunctionParameter<T>? ParseParameterVariable<T>(string value, Source source, string function) where T : notnull, IParsable<T>
	{
		if (VariableManager.ValidName(value, retrieval_only: true))
		{
			return new FunctionParameter<T>(variable_name: value);
		}

		var parameter_value = ParseParameter<T>(value, source, function);

		return parameter_value;
	}

	public static FunctionParameter<T>? ParseEquation<T>(string value, Source source, string function) where T : notnull, IParsable<T>
	{
		if (typeof(T) == typeof(float) || typeof(T) == typeof(int))
		{
			if (FloatRegex.IsMatch(value))
			{
				return ParseParameterVariable<T>(value, source, function);
			}
		}

		if (OperatorDefs.OperatorRegex.IsMatch(value))
		{
			var postfix = ParameterEvaluator.Postfix<T>(source, function, value);

			if (postfix == null)
			{
				return null;
			}

			return new FunctionParameter<T>(postfix);
		}

		return ParseParameterVariable<T>(value, source, function);
	}

	private static ArrayParameter<T>? ParseArrayParameter<T>(string value, Source source, string function) where T : notnull, IParsable<T>
	{
		if (value[0] == '[' && value[^1] == ']')
		{
			value = value[1..^1];
		}

		List<FunctionParameter<T>> parameters = [];

		foreach (Match match in ArraySplitRegex.Matches(value))
		{
			FunctionParameter<T>? function_parameter = ParseParameter<T>(match.Value.Trim(), source, function);

			if (!function_parameter.HasValue)
			{
				return null;
			}

			parameters.Add(function_parameter.Value);
		}

		return new ArrayParameter<T>(parameters);
	}

	public static (object, Type)? ProcessUnknownParameter(string value, Source source, string function)
	{

		if (StringRegex.IsMatch(value))
		{
			return (new FunctionParameter<string>(value: value), typeof(FunctionParameter<string>));
		}

		if (FloatRegex.IsMatch(value))
		{
			var parameter = ParseParameter<float>(value, source, function);

			if (!parameter.HasValue)
			{
				return null;
			}

			return (parameter.Value, typeof(FunctionParameter<float>));
		}

		if (BoolRegex.IsMatch(value))
		{
			var parameter = ParseParameter<bool>(value, source, function);

			if (!parameter.HasValue)
			{
				return null;
			}

			return (parameter.Value, typeof(FunctionParameter<bool>));
		}

		if (VariableManager.ContainsVariable(value, out VariableType type))
		{
			switch (type)
			{
				case VariableType.Int:
					return (new FunctionParameter<int>(value), typeof(FunctionParameter<int>));

				case VariableType.Float:
					return (new FunctionParameter<float>(value), typeof(FunctionParameter<float>));

				case VariableType.String:
					return (new FunctionParameter<string>(variable_name: value), typeof(FunctionParameter<string>));

				case VariableType.Bool:
					return (new FunctionParameter<bool>(value), typeof(FunctionParameter<bool>));

				default:
					break;
			}
		}

		if (OperatorDefs.OperatorRegex.IsMatch(value))
		{
			var equation = ParseEquation<string>(value, source, function);

			if (!equation.HasValue)
			{
				return null;
			}

			return (equation.Value, typeof(FunctionParameter<string>));
		}

		DebugConsole.Raise(new ParameterTypeError(source, function, value, "n\\a", "Unable to parse down to type or variable"));
		return null;
	}

	public static List<object>? ProcessParameters(Source source, string function_name, List<string> inputs, Type[] types)
	{
		List<object> args = [];

		bool parse_variable<T1>(string input, bool equation = false) where T1 : notnull, IParsable<T1> 
		{
			var param = equation ? ParseEquation<T1>(input, source, function_name) : ParseParameter<T1>(input, source, function_name);

			if (!param.HasValue)
			{
				return false;
			}

			args.Add(param.Value);
			return true;
		}

		// parse inputs
		for (int j = 0; j < types.Length; j++)
		{
			byte type_indexer = TypeIndexers.GetTypeIndexer(types[j]);

			switch (type_indexer)
			{
				case TypeIndexers.INT:
					if (!parse_variable<int>(inputs[j], true))
					{
						return null;
					}
					break;

				case TypeIndexers.FLOAT:
					if (!parse_variable<float>(inputs[j], true))
					{
						return null;
					}
					break;

				case TypeIndexers.STRING:
					if (StringRegex.IsMatch(inputs[j].Trim()))
					{
						args.Add(new FunctionParameter<string>(value: inputs[j].Trim()));
						break;
					}

					if (!parse_variable<string>(inputs[j], true))
					{
						return null;
					}
					break;

				case TypeIndexers.BOOL:
					if (!parse_variable<bool>(inputs[j], true))
					{
						return null;
					}
					break;

				case TypeIndexers.TILE_LAYER_TYPE: 
					TileLayerType layer_type = GetLayerType(inputs[j]);
					if (layer_type == TileLayerType.None)
					{
						DebugConsole.Raise(new ParameterTypeError(source, function_name, inputs[j], "TileLayerType"));
						return null;
					}

					args.Add(new FunctionParameter<TileLayerType>(layer_type));
					break;

				case TypeIndexers.USHORT:
					if (!parse_variable<ushort>(inputs[j]))
					{
						return null;
					}
					break;

				case TypeIndexers.UINT:
					if (!parse_variable<uint>(inputs[j]))
					{
						return null;
					}
					break;

				case TypeIndexers.BYTE:
					if (!parse_variable<byte>(inputs[j]))
					{
						return null;
					}
					break;

				case TypeIndexers.RELATIVE_INT:
					bool relative_int = inputs[j].StartsWith('~');

					string relative_int_input = inputs[j][(relative_int ? 1 : 0)..];

					var relative_int_param = ParseEquation<int>(relative_int_input, source, function_name);
					
					if (!relative_int_param.HasValue)
					{
						return null;
					}

					args.Add(new RelativeVariable<FunctionParameter<int>>(relative_int_param.Value, relative_int));
					break;

				case TypeIndexers.RELATIVE_FLOAT:
					bool relative_float = inputs[j].StartsWith('~');

					string relative_float_input = inputs[j][(relative_float ? 1 : 0)..];

					var relative_float_param = ParseEquation<float>(relative_float_input, source, function_name);
					
					if (!relative_float_param.HasValue)
					{
						return null;
					}

					args.Add(new RelativeVariable<FunctionParameter<float>>(relative_float_param.Value, relative_float));
					break;

				case TypeIndexers.TYPE:
					VariableType variable_type = VariableManager.GetVariableType(inputs[j]);
					if (variable_type == VariableType.None)
					{
						DebugConsole.Raise(new ParameterTypeError(source, function_name, inputs[j], "VariableType"));
						return null;
					}

					args.Add(new FunctionParameter<VariableType>(variable_type));
					break;

				case TypeIndexers.OBJECT:
					args.Add(new FunctionParameter<string>(value:inputs[j]));
					break;

				case TypeIndexers.VARIABLE_OBJECT:
					var variable_parameter = ProcessUnknownParameter(inputs[j], source, function_name);

					if (variable_parameter == null)
					{
						return null;
					}

					args.Add(variable_parameter.Value.Item1);
					break;

				case TypeIndexers.DIRECTION:
					Direction direction = GetDirection(inputs[j]);
					if (direction == Direction.None)
					{
						DebugConsole.Raise(new ParameterTypeError(source, function_name, inputs[j], "Direction"));
						return null;
					}

					args.Add(new FunctionParameter<Direction>(direction));
					break;

				case >=0b10000000: // array parameter
					
					bool parse_array_parameter<T1>(string input) where T1 : notnull, IParsable<T1>
					{
						ArrayParameter<T1>? array = ParseArrayParameter<T1>(input, source, function_name);
						if (!array.HasValue)
						{
							DebugConsole.Raise(new ParameterTypeError(source, function_name, input, typeof(T1).Name));
							return false;
						}

						args.Add(array.Value);
						return true;
					}

					// remove array marker
					if (!((type_indexer & 0b01111111) switch
					{
						TypeIndexers.INT => parse_array_parameter<int>(inputs[j]),
						TypeIndexers.FLOAT => parse_array_parameter<float>(inputs[j]),
						TypeIndexers.STRING => parse_array_parameter<string>(inputs[j]),
						TypeIndexers.BOOL => parse_array_parameter<bool>(inputs[j]),
						TypeIndexers.USHORT => parse_array_parameter<ushort>(inputs[j]),
						TypeIndexers.UINT => parse_array_parameter<uint>(inputs[j]),
						TypeIndexers.BYTE => parse_array_parameter<byte>(inputs[j]),
						_ => true
					}))
					{
						throw new NotImplementedException();
					}
					
					break;

				default:
					break;
			}
		}

		return args;
	}


	private static readonly Regex ArraySplitRegex = _arraySplitRegex();
	[GeneratedRegex(@"(?<=^|,)[^,]+", RegexOptions.Compiled)]
	private static partial Regex _arraySplitRegex();
	

	private static readonly Regex StringRegex = _stringRegex();

	[GeneratedRegex(@"^""((\\"")|[^""])*""$", RegexOptions.Compiled)]
	private static partial Regex _stringRegex();

	private static readonly Regex FloatRegex = _floatRegex();

	[GeneratedRegex(@"^[-]?[0-9]*[.]?[0-9]+$", RegexOptions.Compiled)]
	private static partial Regex _floatRegex();

	private static readonly Regex BoolRegex = _boolRegex();

	[GeneratedRegex(@"^(true|false)$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
	private static partial Regex _boolRegex();
}