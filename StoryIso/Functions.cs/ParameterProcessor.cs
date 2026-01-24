using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
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

	private static Direction GetDirection(string name)
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

	static readonly Dictionary<Type, int> typeDict = new Dictionary<Type, int>()
	{
		{typeof(int), 0},
		{typeof(float), 1},
		{typeof(string), 2},
		{typeof(bool), 3},
		{typeof(TileLayerType), 4},
		{typeof(ushort), 5},
		{typeof(uint), 6},
		{typeof(byte), 7},
		{typeof(RelativeVariable<int>), 8},
		{typeof(RelativeVariable<float>), 9},
		{typeof(VariableType), 10},
		{typeof(object), 11},
		{typeof(VariableObject), 12},
		{typeof(Direction), 13},
		{typeof(uint[]), 14}
	};

	public static FunctionParameter<T?>? ParseParameter<T>(string value, string type_name, Source source, string function) where T : struct, IParsable<T>
	{
		if (!T.TryParse(value, null, out T parsed_value))
		{
			DebugConsole.Raise(new ParameterTypeError(source, function, value, type_name));
			return null;
		}

		return new FunctionParameter<T?>(parsed_value);
	}

	public static FunctionParameter<T?>? ParseParameterVariable<T>(string value, string type_name, Source source, string function) where T : struct, IParsable<T>
	{
		if (VariableManager.ValidName(value, retrieval_only: true))
		{
			return new FunctionParameter<T?>(null, value);
		}

		var parameter_value = ParseParameter<T>(value, type_name, source, function);

		return parameter_value;
	}

	private static ArrayParameter<T?>? ParseArrayParameter<T>(string value, string type_name, Source source, string function) where T : struct, IParsable<T>
	{
		if (value[0] == '[' && value[^1] == ']')
		{
			value = value[1..^1];
		}

		List<FunctionParameter<T?>> parameters = [];

		foreach (Match match in ArraySplitRegex.Matches(value))
		{
			FunctionParameter<T?>? function_parameter = ParseParameter<T>(match.Value.Trim(), type_name, source, function);

			if (!function_parameter.HasValue)
			{
				return null;
			}

			parameters.Add(function_parameter.Value);
		}

		return new ArrayParameter<T?>(parameters);
	}

	public static List<object> ProcessParameters(Source source, string function_name, List<string> inputs, Type[] types)
	{
		List<object> args = [];

		// parse inputs
		for (int j = 0; j < types.Length; j++)
		{
			switch (typeDict[types[j]])
			{
				case 0: // int
					var int_param = ParseParameterVariable<int>(inputs[j], "int", source, function_name);
					
					if (!int_param.HasValue)
					{
						return null;
					}

					args.Add(int_param.Value);
					break;

				case 1: // float
					var float_param = ParseParameterVariable<float>(inputs[j], "float", source, function_name);
					
					if (!float_param.HasValue)
					{
						return null;
					}

					args.Add(float_param.Value);
					break;

				case 2: // string
					if (!(inputs[j][0] == '\"' &&
						inputs[j][^1] == '\"'))
					{
						if (!VariableManager.ValidName(inputs[j]))
						{
							DebugConsole.Raise(new ParameterTypeError(source, function_name, inputs[j], "string"));
							return null;
						}

						args.Add(new FunctionParameter<string>(null, inputs[j]));
						break;
					}

					args.Add(new FunctionParameter<string>(inputs[j]));
					break;

				case 3: // bool
					var bool_param = ParseParameterVariable<bool>(inputs[j], "bool", source, function_name);
					
					if (!bool_param.HasValue)
					{
						return null;
					}

					args.Add(bool_param.Value);
					break;

				case 4: // TileLayerType
					TileLayerType layer_type = GetLayerType(inputs[j]);
					if (layer_type == TileLayerType.None)
					{
						DebugConsole.Raise(new ParameterTypeError(source, function_name, inputs[j], "TileLayerType"));
						return null;
					}

					args.Add(new FunctionParameter<TileLayerType>(layer_type));
					break;

				case 5: // ushort 
					var ushort_param = ParseParameter<ushort>(inputs[j], "ushort", source, function_name);
					
					if (!ushort_param.HasValue)
					{
						return null;
					}

					args.Add(ushort_param.Value);
					break;

				case 6: // uint 
					var uint_param = ParseParameter<uint>(inputs[j], "uint", source, function_name);
					
					if (!uint_param.HasValue)
					{
						return null;
					}

					args.Add(uint_param.Value);
					break;

				case 7: // byte 
					var byte_param = ParseParameter<byte>(inputs[j], "byte", source, function_name);
					
					if (!byte_param.HasValue)
					{
						return null;
					}

					args.Add(byte_param.Value);
					break;

				case 8: // relative int 
					bool relative_int = inputs[j].StartsWith('~');

					string relative_int_input = inputs[j][(relative_int ? 1 : 0)..];

					var relative_int_param = ParseParameterVariable<int>(relative_int_input, "int", source, function_name);
					
					if (!relative_int_param.HasValue)
					{
						return null;
					}

					args.Add(new RelativeVariable<FunctionParameter<int?>>(relative_int_param.Value, relative_int));
					break;

				case 9: // relative float 
					bool relative_float = inputs[j].StartsWith('~');

					string relative_float_input = inputs[j][(relative_float ? 1 : 0)..];

					var relative_float_param = ParseParameterVariable<float>(relative_float_input, "float", source, function_name);
					
					if (!relative_float_param.HasValue)
					{
						return null;
					}

					args.Add(new RelativeVariable<FunctionParameter<float?>>(relative_float_param.Value, relative_float));
					break;

				case 10: // variable type
					VariableType variable_type = VariableManager.GetVariableType(inputs[j]);
					if (variable_type == VariableType.None)
					{
						DebugConsole.Raise(new ParameterTypeError(source, function_name, inputs[j], "VariableType"));
						return null;
					}

					args.Add(new FunctionParameter<VariableType>(variable_type));
					break;

				case 11: // object 
					args.Add(new FunctionParameter<string>(inputs[j]));
					break;

				case 12: // variable object
					if (VariableManager.ValidName(inputs[j]))
					{
						args.Add(new FunctionParameter<object>(null, inputs[j]));
						break;
					}

					args.Add(new FunctionParameter<object>(inputs[j]));
					break;

				case 13: // Direction
					Direction direction = GetDirection(inputs[j]);
					if (direction == Direction.None)
					{
						DebugConsole.Raise(new ParameterTypeError(source, function_name, inputs[j], "Direction"));
						return null;
					}

					args.Add(new FunctionParameter<Direction>(direction));
					break;

				case 14: // uint[]
					ArrayParameter<uint?>? uints = ParseArrayParameter<uint>(inputs[j], "uint", source, function_name);
					if (!uints.HasValue)
					{
						DebugConsole.Raise(new ParameterTypeError(source, function_name, inputs[j], "uint[]"));
						return null;
					}

					args.Add(uints.Value);
					break;

				default:
					break;
			}
		}

		return args;
	}


	[GeneratedRegex(@"(?<=^|,)[^,]+", RegexOptions.Compiled)]
	private static partial Regex _arraySplitRegex();
	private static Regex ArraySplitRegex = _arraySplitRegex();
}