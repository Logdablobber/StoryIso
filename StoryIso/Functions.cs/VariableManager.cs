using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using MonoGame.Extended;
using StoryIso.Audio;
using StoryIso.Debugging;
using StoryIso.Enums;
using StoryIso.Misc;

namespace StoryIso.Functions;

public static partial class VariableManager
{
	private static readonly Dictionary<string, Optional<int>> _intVariables = [];
	private static readonly Dictionary<string, Optional<float>> _floatVariables = [];
	private static readonly Dictionary<string, Optional<bool>> _boolVariables = [];
	private static readonly Dictionary<string, Optional<string>> _stringVariables = [];

	public readonly static Dictionary<string, VariableType> _typesOfVariables = new();

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

	public static VariableType GetVariableType(Type type)
	{
		if (typeToVariableType.TryGetValue(type, out var variableType))
		{
			return variableType;
		}

		return VariableType.None;
	}

	readonly static Dictionary<string, Func<int>> _readonlyInts = new()
	{
		
	};

	readonly static Dictionary<string, Func<float>> _readonlyFloats = new()
	{
		{"playerX", () => Game1.tiledManager.WorldXToTileX(Game1.player.Get<Transform2>().Position.X)},
		{"playerY", () => Game1.tiledManager.WorldYToTileY(Game1.player.Get<Transform2>().Position.Y)},
		{"masterVolume", AudioManager.GetVolume},
	};

	readonly static Dictionary<string, Func<string>> _readonlyStrings = new()
	{
		{"bgmName", () => AudioManager.BGMName ?? "N/A"},
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

	public static void Initialize()
	{
		foreach (var readonly_int in _readonlyInts.Keys)
		{
			_typesOfVariables.Add(readonly_int, VariableType.Int);
		}

		foreach (var readonly_float in _readonlyFloats.Keys)
		{
			if (_typesOfVariables.ContainsKey(readonly_float))
			{
				_typesOfVariables[readonly_float] = VariableType.Float;
				continue;
			}

			_typesOfVariables.Add(readonly_float, VariableType.Float);
		}

		foreach (var readonly_bool in _readonlyBools.Keys)
		{
			_typesOfVariables.Add(readonly_bool, VariableType.Bool);
		}

		foreach (var readonly_string in _readonlyStrings.Keys)
		{
			_typesOfVariables.Add(readonly_string, VariableType.String);
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
				_intVariables[name] = (Optional<int>)(value ?? default(Optional<int>));
				break;

			case VariableType.Float:
				_floatVariables[name] = (Optional<float>)(value ?? default(Optional<float>));
				break;

			case VariableType.String:
				_stringVariables[name] = (Optional<string>)(value ?? default(Optional<string>));
				break;

			case VariableType.Bool:
				_boolVariables[name] = (Optional<bool>)(value ?? default(Optional<bool>));
				break;

			default:
				throw new NotImplementedException();
		}

		_typesOfVariables.Add(name, type);
	}

	public static T? GetVariable<T>(string name, Source source) where T : notnull
	{
		if (_typesOfVariables.TryGetValue(name, out var type))
		{
			var retrieved_type = GetVariableType(typeof(T));

			if (retrieved_type == VariableType.None)
			{
				throw new NotImplementedException($"Variable type {typeof(T).Name} not implemented yet");
			}

			// check if the retrieved type is the same as the actual variable type
			// or if you can convert the variable to the retrieved type
			if (!(retrieved_type == type ||
				retrieved_type == VariableType.String ||
				(retrieved_type == VariableType.Int && type == VariableType.Float) ||
				(retrieved_type == VariableType.Float && type == VariableType.Int)))
			{
				DebugConsole.Raise(new WrongVariableTypeError(source, name, typeof(T).Name));
			}
			
			object? get_var<T1>(Dictionary<string, Func<T1>> readonly_vars, Dictionary<string, Optional<T1>> vars) where T1 : notnull
			{
				if (readonly_vars.TryGetValue(name, out var function))
				{
					return function();
				}

				Optional<T1> value = vars[name];

				if (!value.HasValue)
				{
					DebugConsole.Raise(new UndefinedVariableError(source, name));
					return null;
				}

				return value.Value;
			}

			object? res = type switch
			{
				VariableType.Int => get_var(_readonlyInts, _intVariables),
				VariableType.Float => get_var(_readonlyFloats, _floatVariables),
				VariableType.String => get_var(_readonlyStrings, _stringVariables),
				VariableType.Bool => get_var(_readonlyBools, _boolVariables),
				_ => throw new NotImplementedException(),
			};
			
			if (res == null)
			{
				return default;
			}

			if (retrieved_type == type)
			{
				return (T)res;
			}

			// conversions!
			if (retrieved_type == VariableType.String)
			{
				switch (type)
				{
					case VariableType.Int:
						int int_res = (int)res;

						return (T)(object)int_res.ToString();

					case VariableType.Float:
						float float_res = (float)res;

						return (T)(object)float_res.ToString();

					case VariableType.Bool:
						bool bool_res = (bool)res;

						return (T)(object)bool_res.ToString();

					default:
						throw new NotImplementedException();
				}
			}

			if (retrieved_type == VariableType.Int && type == VariableType.Float)
			{
				int int_res = (int)res;

				return (T)(object)(float)int_res;
			}

			if (retrieved_type == VariableType.Float && type == VariableType.Int)
			{
				float float_res = (float)res;

				return (T)(object)(int)float_res;
			}

			throw new UnreachableException("How tf did you get here?");
		}

		DebugConsole.Raise(new UnknownVariableError(source, name));
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

		if (_intVariables.TryGetValue(name, out Optional<int> int_value))
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

		if (_floatVariables.TryGetValue(name, out Optional<float> float_value))
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

		if (_boolVariables.TryGetValue(name, out Optional<bool> bool_value))
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

		if (_stringVariables.TryGetValue(name, out Optional<string> string_value))
		{
			type = VariableType.String;
			value = string_value;
			return true;
		}

		type = VariableType.None;
		value = null;
		return false;
	}

	public static bool TryGetVariableAsString(string name, out string? value)
	{
		if (_readonlyInts.TryGetValue(name, out Func<int>? readonly_int_value))
		{
			value = readonly_int_value().ToString();
			return true;
		}

		if (_intVariables.TryGetValue(name, out Optional<int> int_value))
		{
			if (!int_value.HasValue)
			{
				value = null;
				return false;
			}

			value = int_value.ToString();
			return true;
		}

		if (_readonlyFloats.TryGetValue(name, out Func<float>? readonly_float_value))
		{
			value = readonly_float_value().ToString();
			return true;
		}

		if (_floatVariables.TryGetValue(name, out Optional<float> float_value))
		{
			if (!float_value.HasValue)
			{
				value = null;
				return false;
			}

			value = float_value.ToString();
			return true;
		}

		if (_readonlyBools.TryGetValue(name, out Func<bool>? readonly_bool_value))
		{
			value = readonly_bool_value().ToString();
			return true;
		}

		if (_boolVariables.TryGetValue(name, out Optional<bool> bool_value))
		{
			if (!bool_value.HasValue)
			{
				value = null;
				return false;
			}

			value = bool_value.ToString();
			return true;
		}

		if (_readonlyStrings.TryGetValue(name, out Func<string>? readonly_string_value))
		{
			value = readonly_string_value();
			return true;
		}

		if (_stringVariables.TryGetValue(name, out Optional<string> string_value))
		{
			if (!string_value.HasValue)
			{
				value = null;
				return false;
			}

			value = string_value.Value;
			return true;
		}

		value = null;
		return false;
	}

	public static bool ContainsVariable(string name, out VariableType type)
	{
		if (_typesOfVariables.TryGetValue(name, out type))
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
				_stringVariables[name] = default;
			}

			_stringVariables[name] = string_value.Value;
			return;
		}

		if (_intVariables.ContainsKey(name))
		{
			Optional<int> int_value = ParseVariable<int>(value);

			if (!int_value.HasValue)
			{
				_intVariables[name] = default;
			}

			_intVariables[name] = int_value.Value;
			return;
		}

		if (_floatVariables.ContainsKey(name))
		{
			Optional<float> float_value = ParseVariable<float>(value);

			if (!float_value.HasValue)
			{
				_floatVariables[name] = default;
			}

			_floatVariables[name] = float_value.Value;
			return;
		}

		if (_boolVariables.ContainsKey(name))
		{
			Optional<bool> bool_value = ParseVariable<bool>(value);

			if (!bool_value.HasValue)
			{
				_boolVariables[name] = default;
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