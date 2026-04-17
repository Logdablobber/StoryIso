using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using MonoGame.Extended;
using StoryIso.Audio;
using StoryIso.Debugging;
using StoryIso.Enums;
using StoryIso.Misc;
using StoryIso.Scripting.Variables;

namespace StoryIso.Scripting.Variables;

public static partial class VariableManager
{
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

	public static string GetVariableTypeName(VariableType type)
	{
		return type switch
		{
			VariableType.Int => "int",
			VariableType.Float => "float",
			VariableType.Bool => "bool",
			VariableType.String => "string",
			_ => "nullType",
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

	readonly static Dictionary<string, Func<bool>> _readonlyBools = new()
	{
		{"movementLocked", () => Game1.sceneManager.Active},
	};

	readonly static Dictionary<string, float> _constantFloats = new()
	{
		{"pi", 3.14159265358979f},
	};

	readonly static string[] _invalidNames =
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

	public static void AddGlobalConstant<T>(Source source, string name, T value) where T : notnull
	{
		var new_constant = new ConstantVariable<T>(name, value);

		Game1.GlobalScope.DefineVariable(source, new_constant);
	}

	private static void AddGlobalReadOnlyVariable<T>(Source source, string name, Func<Optional<T>> value) where T : notnull
	{
		var new_variable = new ReadOnlyVariable<T>(name, value);

		Game1.GlobalScope.DefineVariable(source, new_variable);
	}

	public static void AddGlobalVariable<T>(Source source, string name, Optional<T> value) where T : notnull
	{
		var new_variable = new ValueVariable<T>(name, value);

		Game1.GlobalScope.DefineVariable(source, new_variable);
	}

	public static void Initialize()
	{
		Source source = new(0, null, "VariableManager.Initialize");

		// define readonly variables here:
		AddGlobalReadOnlyVariable<float>(source, "playerX", () => Game1.tiledManager.WorldXToTileX(Game1.player?.Get<Transform2>().Position.X).ToOptional());
		AddGlobalReadOnlyVariable<float>(source, "playerY", () => Game1.tiledManager.WorldYToTileY(Game1.player?.Get<Transform2>().Position.Y).ToOptional());
		AddGlobalReadOnlyVariable<float>(source, "masterVolume", () => AudioManager.GetVolume());

		AddGlobalReadOnlyVariable<string>(source, "bgmName", () => AudioManager.BGMName ?? "N/A");

		AddGlobalReadOnlyVariable<bool>(source, "movementLocked", () => Game1.sceneManager.Active);

		LoadConstants();
	}

	private static void LoadConstants()
	{
		const string CONSTANTS_PATH = "./Content/System/Constants.json";

		var file_info = new FileInfo(CONSTANTS_PATH);

		if (!new FileInfo(CONSTANTS_PATH).Exists)
		{
			throw new FileNotFoundException("Constants json not found");
		}

		Source source = new Source(0, null, "VariableManager.LoadConstants");

		string json_text;
		using (StreamReader streamReader = new(CONSTANTS_PATH))
		{
			json_text = streamReader.ReadToEnd();
		}

		var constants_data = JsonSerializer.Deserialize<ConstantsData>(json_text);

		foreach (var constant in constants_data.Constants)
		{
			string name = constant.Name;

			if (Game1.GlobalScope.ContainsVariable(name, out _))
			{
				DebugConsole.Raise(new VariableAlreadyExistsError(source, name, "Constant or Variable by that name already exists"));
				continue;
			}

			switch (constant)
			{
				case FloatConstant floatConstant:
					AddGlobalConstant(source, name, floatConstant.Value);
					break;

				case IntConstant intConstant:
					AddGlobalConstant(source, name, intConstant.Value);
					break;

				case StringConstant stringConstant:
					AddGlobalConstant(source, name, stringConstant.Value);
					break;

				case BoolConstant boolConstant:
					AddGlobalConstant(source, name, boolConstant.Value);
					break;

				default:
					throw new NotImplementedException();
			}
		}
	}

	public static void DefineVariable(Source source, Scope scope, List<object> parameters)
	{
		Optional<VariableType> type = ParameterProcessor.Convert<VariableType>(source, parameters[0]);
		Optional<string> name = ParameterProcessor.Convert<string>(source, parameters[1]);
		IOptional? value = ParameterProcessor.ConvertUnknown(source, parameters[2]);

		if (!type.HasValue || !name.HasValue)
		{
			return;
		}

		IVariable create<T>() where T : notnull
		{
			if (value == null)
			{
				return new ValueVariable<T>(name.Value, default);
			}

			return new ValueVariable<T>(name.Value, ParameterProcessor.ConvertOptional<T>(source, value));
		}

		var new_variable = type.Value switch
		{
			VariableType.Int => create<int>(),
			VariableType.Float => create<float>(),
			VariableType.String => create<string>(),
			VariableType.Bool => create<bool>(),
			_ => throw new UnreachableException(),
		};
	
		scope.DefineVariable(source, new_variable);
	}
}