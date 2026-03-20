using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StoryIso.Scripting;

public static partial class FunctionDefs
{
	private static readonly Dictionary<string, ushort> NameToFunctionDefIndex = [];
	private static readonly Dictionary<ushort, FunctionDef> IndexToFunctionDef = [];

	public static ushort GOTOIF_Index;

	public static void Initialize()
	{
		// link all of the operators defined in ./Functions/Operators/
		// using System.Reflection!
		var fields = typeof(FunctionDefs).GetFields(BindingFlags.NonPublic | BindingFlags.Static).Where((field) => field.FieldType == typeof(FunctionDef)).ToArray();

		for (ushort i = 0; i < fields.Length; i++)
		{
			var value = fields[i].GetValue(null);

			if (value == null)
			{
				continue;
			}

			var def = (FunctionDef)value;

			IndexToFunctionDef.Add(i, def);

			if (def.name == null)
			{
				continue;
			}

			if (def.name == "GOTOIF")
			{
				GOTOIF_Index = i;
				continue;
			}

			NameToFunctionDefIndex.Add(def.name, i);
		}
	}


	public static FunctionDef Get(string name)
	{
		if (NameToFunctionDefIndex.TryGetValue(name, out var value))
		{
			return IndexToFunctionDef[value];
		}

		return FunctionDefs.None; // null function
	}

	public static FunctionDef Get(ushort index)
	{
		return IndexToFunctionDef[index];
	}

	public static ushort GetIndex(string name)
	{
		for (ushort i = 0; i < IndexToFunctionDef.Count; i++)
		{
			if (IndexToFunctionDef[i].name == name)
			{
				return i;
			}
		}

		return 0;
	}
}