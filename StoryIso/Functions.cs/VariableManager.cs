using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MonoGame.Extended;
using StoryIso.Debugging;
using StoryIso.Enums;
using StoryIso.Misc;

namespace StoryIso.Functions;

public static partial class VariableManager
{
	private static Dictionary<string, int?> _intVariables = [];
	private static Dictionary<string, float?> _floatVariables = [];
	private static Dictionary<string, bool?> _boolVariables = [];
	private static Dictionary<string, string?> _stringVariables = [];

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

	private static readonly Dictionary<Type, VariableType> typeToVariableType = new()
	{
		{typeof(int), VariableType.Int},
		{typeof(float), VariableType.Float},
		{typeof(bool), VariableType.Bool},
		{typeof(string), VariableType.String}
	};

	public static VariableType? GetVariableType(Type type)
	{
		if (typeToVariableType.TryGetValue(type, out var variableType))
		{
			return variableType;
		}

		return VariableType.None;
	}

	readonly static Dictionary<string, Func<int>> _readonlyInts = new()
	{
		{"playerX", () => (int)Game1.tiledManager.WorldXToTileX(Game1.player.Get<Transform2>().Position.X)},
		{"playerY", () => (int)Game1.tiledManager.WorldYToTileY(Game1.player.Get<Transform2>().Position.Y)}
	};

	readonly static Dictionary<string, Func<float>> _readonlyFloats = new()
	{
		{"playerX", () => Game1.tiledManager.WorldXToTileX(Game1.player.Get<Transform2>().Position.X)},
		{"playerY", () => Game1.tiledManager.WorldYToTileY(Game1.player.Get<Transform2>().Position.Y)}
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

	public readonly static Dictionary<string, VariableType> VariableTypes = new();

	public static void Initialize()
	{
		foreach (var readonly_int in _readonlyInts.Keys)
		{
			VariableTypes.Add(readonly_int, VariableType.Int);
		}

		foreach (var readonly_float in _readonlyFloats.Keys)
		{
			if (VariableTypes.ContainsKey(readonly_float))
			{
				VariableTypes[readonly_float] = VariableType.Float;
				continue;
			}

			VariableTypes.Add(readonly_float, VariableType.Float);
		}

		foreach (var readonly_bool in _readonlyBools.Keys)
		{
			VariableTypes.Add(readonly_bool, VariableType.Bool);
		}

		foreach (var readonly_string in _readonlyStrings.Keys)
		{
			VariableTypes.Add(readonly_string, VariableType.String);
		}
	}

	public static bool ValidName(string name, bool retrieval_only = false)
	{
		if (_constantNames.Contains(name) ||
			(!retrieval_only && _retrievalOnlyVariables.Contains(name)))
		{
			return false;
		}

		return VariableRegex.IsMatch(name);
	}

	public static void DefineVariable(VariableType type, string name, object? value, Source source)
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
				_intVariables[name] = (int?)value;
				break;

			case VariableType.Float:
				_floatVariables[name] = (float?)value;
				break;

			case VariableType.String:
				_stringVariables[name] = (string?)value;
				break;

			case VariableType.Bool:
				_boolVariables[name] = (bool?)value;
				break;

			default:
				return; // don't add if type is undefined
		}

		VariableTypes.Add(name, type);
	}

	public static T? GetVariable<T>(string name, Source source) where T : notnull
	{
		if (typeof(T) == typeof(int))
		{
			if (_readonlyInts.TryGetValue(name, out var int_function))
			{
				return (T?)(object)int_function();
			}

			if (!_intVariables.TryGetValue(name, out int? int_value) || !int_value.HasValue)
			{
				DebugConsole.Raise(new UnknownVariableError(source, name));
				return default;
			}

			return (T?)(object)int_value.Value;
		}

		if (typeof(T) == typeof(float))
		{
			if (_readonlyFloats.TryGetValue(name, out var float_function))
			{
				return (T?)(object)float_function();
			}

			if (!_floatVariables.TryGetValue(name, out float? float_value) || !float_value.HasValue)
			{
				DebugConsole.Raise(new UnknownVariableError(source, name));
				return default;
			}

			return (T?)(object)float_value.Value;
		}

		if (typeof(T) == typeof(string))
		{
			if (_readonlyStrings.TryGetValue(name, out var string_function))
			{
				return (T?)(object)string_function();
			}

			if (!_stringVariables.TryGetValue(name, out string? string_value) || string_value == null)
			{
				DebugConsole.Raise(new UnknownVariableError(source, name));
				return default;
			}

			return (T?)(object)string_value;
		}

		if (typeof(T) == typeof(bool))
		{
			if (_readonlyBools.TryGetValue(name, out var bool_function))
			{
				return (T?)(object)bool_function();
			}

			if (!_boolVariables.TryGetValue(name, out bool? bool_value) || !bool_value.HasValue)
			{
				DebugConsole.Raise(new UnknownVariableError(source, name));
				return default;
			}

			return (T?)(object)bool_value.Value;
		}

		return default;
	}

	public static bool TryGetVariable(string name, out VariableType type, out object? value)
	{
		if (_readonlyInts.TryGetValue(name, out Func<int>? readonly_int_value))
		{
			type = VariableType.Int;
			value = (int?)readonly_int_value();
			return true;
		}

		if (_intVariables.TryGetValue(name, out int? int_value))
		{
			type = VariableType.Int;
			value = int_value;
			return true;
		}

		if (_readonlyFloats.TryGetValue(name, out Func<float>? readonly_float_value))
		{
			type = VariableType.Float;
			value = (float?)readonly_float_value();
			return true;
		}

		if (_floatVariables.TryGetValue(name, out float? float_value))
		{
			type = VariableType.Float;
			value = float_value;
			return true;
		}

		if (_readonlyBools.TryGetValue(name, out Func<bool>? readonly_bool_value))
		{
			type = VariableType.Bool;
			value = (bool?)readonly_bool_value();
			return true;
		}

		if (_boolVariables.TryGetValue(name, out bool? bool_value))
		{
			type = VariableType.Bool;
			value = bool_value;
			return true;
		}

		if (_readonlyStrings.TryGetValue(name, out Func<string>? readonly_string_value))
		{
			type = VariableType.String;
			value = readonly_string_value();
			return true;
		}

		if (_stringVariables.TryGetValue(name, out string? string_value))
		{
			type = VariableType.String;
			value = string_value;
			return true;
		}

		type = VariableType.None;
		value = null;
		return false;
	}

	public static bool ContainsVariable(string name, out VariableType type)
	{
		if (VariableTypes.TryGetValue(name, out type))
		{
			return true;
		}

		type = VariableType.None;
		return false;
	}

	public static void SetVariable(string name, object? value, Source source)
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
			var string_value = (Optional<string>)value;

			if (!string_value.HasValue)
			{
				_stringVariables[name] = null;
			}

			_stringVariables[name] = string_value.Value;
			return;
		}

		if (_intVariables.ContainsKey(name))
		{
			Optional<int> int_value = ParseVariable<int>(value);

			if (!int_value.HasValue)
			{
				_intVariables[name] = null;
			}

			_intVariables[name] = int_value.Value;
			return;
		}

		if (_floatVariables.ContainsKey(name))
		{
			Optional<float> float_value = ParseVariable<float>(value);

			if (!float_value.HasValue)
			{
				_floatVariables[name] = null;
			}

			_floatVariables[name] = float_value.Value;
			return;
		}

		if (_boolVariables.ContainsKey(name))
		{
			Optional<bool> bool_value = ParseVariable<bool>(value);

			if (!bool_value.HasValue)
			{
				_boolVariables[name] = null;
			}

			_boolVariables[name] = bool_value.Value;
			return;
		}

		DebugConsole.Raise(new UnknownVariableError(source, name));
	}

	private static Optional<T> ParseVariable<T>(object? value) where T : IParsable<T>
	{
		if (value == null)
		{
			return default;
		}

		if (value is Optional<string> string_value)
		{
			if (!string_value.HasValue)
			{
				return default;
			}

			if (T.TryParse(string_value.Value, null, out T? result))
			{
				return result;
			}

			throw new InvalidCastException();
		}

		var final_value = (Optional<T>)value;

		return final_value;
	}

	public static readonly Regex VariableRegex = _variableRegex();

	[GeneratedRegex(@"^[A-Za-z_]+[A-Za-z0-9_]*$", RegexOptions.Compiled)]
	private static partial Regex _variableRegex();
}