using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MonoGame.Extended;
using StoryIso.Debugging;
using StoryIso.Enums;

namespace StoryIso.Functions;

public static class VariableManager
{
	private static Dictionary<string, int> _intVariables = [];
	private static Dictionary<string, float> _floatVariables = [];
	private static Dictionary<string, bool> _boolVariables = [];
	private static Dictionary<string, string> _stringVariables = [];

	public static Dictionary<string, int> Ints
	{
		get 
		{
			return _intVariables;
		}
	}

	public static Dictionary<string, float> Floats
	{
		get 
		{
			return _floatVariables;
		}
	}

	public static Dictionary<string, bool> Bools
	{
		get 
		{
			return _boolVariables;
		}
	}

	public static Dictionary<string, string> Strings
	{
		get 
		{
			return _stringVariables;
		}
	}

	public static VariableType GetVariableType(string name)
	{
		return name.ToLower() switch
		{
			"int" => VariableType.Int,
			"float" => VariableType.Float,
			"bool" => VariableType.Bool,
			"string" => VariableType.String,
			_ => VariableType.None,
		};
	}

	readonly static Dictionary<string, Func<int>> _readonlyInts = new()
	{
		{"playerX", () => (int)Game1.tiledManager.WorldPosToTilePos(Game1.player.Get<Transform2>().Position).X},
		{"playerY", () => (int)Game1.tiledManager.WorldPosToTilePos(Game1.player.Get<Transform2>().Position).Y}
	};

	readonly static Dictionary<string, Func<float>> _readonlyFloats = new()
	{
		{"playerX", () => Game1.tiledManager.WorldPosToTilePos(Game1.player.Get<Transform2>().Position).X},
		{"playerY", () => Game1.tiledManager.WorldPosToTilePos(Game1.player.Get<Transform2>().Position).Y}
	};

	readonly static Dictionary<string, Func<string>> _readonlyStrings = new()
	{
		
	};

	readonly static Dictionary<string, Func<bool>> _readonlyBools = new()
	{
		{"movementLocked", () => Game1.sceneManager.Active},
	};

	readonly static string[] _retrievalOnlyVariables = _readonlyInts.Keys.Concat(_readonlyStrings.Keys).Concat(_readonlyBools.Keys).ToArray();

	readonly static string[] _constantNames =
	[
		"true",
		"false",
		"True",
		"False",
		"WallLayer",
		"FloorLayer",
		"InteractionTileLayer",
		"Left",
		"Right",
		"Up",
		"Down"
	];

	public readonly static string[] KeywordNames = _retrievalOnlyVariables.Concat(_constantNames).ToArray();

	public static bool ValidName(string name, bool retrieval_only = false)
	{
		if (_constantNames.Contains(name) ||
			(!retrieval_only && _retrievalOnlyVariables.Contains(name)))
		{
			return false;
		}

		const string PATTERN = @"^[A-Za-z_]+[A-Za-z0-9_]*$";

		return Regex.IsMatch(name, PATTERN);
	}

	public static void DefineVariable(VariableType type, string name, string value, Source source)
	{
		if (type == VariableType.None)
		{
			return;
		}

		if (!ValidName(name))
		{
			DebugConsole.Raise(new BadVariableNameError(source, name, "Variable names must be alphanumeric or underscores and cannot start with a number"));
		}

		if (_intVariables.ContainsKey(name) ||
			_floatVariables.ContainsKey(name) ||
			_stringVariables.ContainsKey(name) ||
			_boolVariables.ContainsKey(name))
		{
			DebugConsole.Raise(new VariableAlreadyExistsError(source, name));
			return;
		}

		switch (type)
		{
			case VariableType.Int:
				if (!(int.TryParse(value, out int int_value) || 
						_intVariables.TryGetValue(value, out int_value)))
				{
					DebugConsole.Raise(new WrongVariableTypeError(source, name, "int", value));
					return;
				}

				_intVariables[name] = int_value;
				break;

			case VariableType.Float:
				if (!(float.TryParse(value, out float float_value) ||
						_floatVariables.TryGetValue(value, out float_value)))
				{
					DebugConsole.Raise(new WrongVariableTypeError(source, name, "float", value));
					return;
				}

				_floatVariables[name] = float_value;
				break;

			case VariableType.String:
				string string_value = value;

				if (!((value[0] == '\"' &&
					value[^1] == '\"') || 
					_stringVariables.TryGetValue(value, out string_value)))
				{
					DebugConsole.Raise(new WrongVariableTypeError(source, name, "string", value));
					return;
				}

				_stringVariables[name] = string_value;
				break;

			case VariableType.Bool:
				if (!(bool.TryParse(value, out bool bool_value) ||
						_boolVariables.TryGetValue(value, out bool_value)))
				{
					DebugConsole.Raise(new WrongVariableTypeError(source, name, "bool", value));
					return;
				}

				_boolVariables[name] = bool_value;
				break;

			default:
				break;
		}
	}

	public static T GetVariable<T>(string name, Source source)
	{
		if (typeof(T) == typeof(int?))
		{
			if (_readonlyInts.TryGetValue(name, out var int_function))
			{
				return (T)(object)int_function();
			}

			if (!_intVariables.TryGetValue(name, out int int_value))
			{
				DebugConsole.Raise(new UnknownVariableError(source, name));
				return default;
			}

			return (T)(object)int_value;
		}

		if (typeof(T) == typeof(float?))
		{
			if (_readonlyFloats.TryGetValue(name, out var float_function))
			{
				return (T)(object)float_function();
			}

			if (!_floatVariables.TryGetValue(name, out float float_value))
			{
				DebugConsole.Raise(new UnknownVariableError(source, name));
				return default;
			}

			return (T)(object)float_value;
		}

		if (typeof(T) == typeof(string))
		{
			if (_readonlyStrings.TryGetValue(name, out var string_function))
			{
				return (T)(object)string_function();
			}

			if (!_stringVariables.TryGetValue(name, out string string_value))
			{
				DebugConsole.Raise(new UnknownVariableError(source, name));
				return default;
			}

			return (T)(object)string_value;
		}

		if (typeof(T) == typeof(bool?))
		{
			if (_readonlyBools.TryGetValue(name, out var bool_function))
			{
				return (T)(object)bool_function();
			}

			if (!_boolVariables.TryGetValue(name, out bool bool_value))
			{
				DebugConsole.Raise(new UnknownVariableError(source, name));
				return default;
			}

			return (T)(object)bool_value;
		}

		return default;
	}

	public static bool ContainsVariable(string name, out VariableType type, out object value)
	{
		if (_intVariables.TryGetValue(name, out int int_value))
		{
			type = VariableType.Int;
			value = int_value;
			return true;
		}

		if (_floatVariables.TryGetValue(name, out float float_value))
		{
			type = VariableType.Float;
			value = float_value;
			return true;
		}

		if (_boolVariables.TryGetValue(name, out bool bool_value))
		{
			type = VariableType.Bool;
			value = bool_value;
			return true;
		}

		if (_stringVariables.TryGetValue(name, out string string_value))
		{
			type = VariableType.String;
			value = string_value;
			return true;
		}

		type = VariableType.None;
		value = null;
		return false;
	}

	public static bool ContainsVariable(string name, out string value)
	{
		if (_intVariables.TryGetValue(name, out int int_value))
		{
			value = int_value.ToString();
			return true;
		}

		if (_floatVariables.TryGetValue(name, out float float_value))
		{
			value = float_value.ToString();
			return true;
		}

		if (_boolVariables.TryGetValue(name, out bool bool_value))
		{
			value = bool_value.ToString();
			return true;
		}

		if (_stringVariables.TryGetValue(name, out string string_value))
		{
			value = string_value;
			return true;
		}

		value = null;
		return false;
	}

	public static void SetVariable(string name, string value, Source source)
	{
		if (value == null)
		{
			return;
		}

		if (!ValidName(name))
		{
			return;
		}

		if (_stringVariables.ContainsKey(name))
		{
			if (value[0] != '\"' || value[^1] != '\"')
			{
				DebugConsole.Raise(new ParameterTypeError(source, "SetVar", value, "string"));
				return;
			}

			_stringVariables[name] = value;
			return;
		}

		if (_intVariables.ContainsKey(name))
		{
			if (!int.TryParse(value, out int int_value))
			{
				DebugConsole.Raise(new ParameterTypeError(source, "SetVar", value, "int"));
				return;
			}

			_intVariables[name] = int_value;
			return;
		}

		if (_floatVariables.ContainsKey(name))
		{
			if (!float.TryParse(value, out float float_value))
			{
				DebugConsole.Raise(new ParameterTypeError(source, "SetVar", value, "float"));
				return;
			}

			_floatVariables[name] = float_value;
			return;
		}

		if (_boolVariables.ContainsKey(name))
		{
			if (!bool.TryParse(value, out bool bool_value))
			{
				DebugConsole.Raise(new ParameterTypeError(source, "SetVar", value, "bool"));
				return;
			}

			_boolVariables[name] = bool_value;
			return;
		}

		DebugConsole.Raise(new UnknownVariableError(source, name));
	}

}