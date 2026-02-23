using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using StoryIso.Enums;
using StoryIso.Misc;

namespace StoryIso.Functions;

public static class OperatorDefs
{
	static readonly OperatorDef[] _operators = 
	[
		new OperatorDef
		{
			oper = "!",
			parameters = [typeof(bool)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = (Optional<bool>)args![0];

				if (!item1.HasValue)
				{
					return new Optional<bool>();
				}

				return new Optional<bool>(!item1.Value);
			}
		},
		new OperatorDef
		{
			oper = "^",
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(float),
			function = (args, _) =>
			{
				var item1 = (Optional<float>)args![0];
				var item2 = (Optional<float>)args[1];

				if (!item1.HasValue || !item2.HasValue)
				{
					return new Optional<float>();
				}

				return new Optional<float>(MathF.Pow(item2.Value, item1.Value));
			}
		},
		new OperatorDef // floor
		{
			oper = ",",
			parameters = [typeof(float)],
			returnType = typeof(float),
			function = (args, _) =>
			{
				var item1 = (Optional<float>)args![0];

				if (!item1.HasValue)
				{
					return new Optional<float>();
				}

				return new Optional<float>(MathF.Floor(item1.Value));
			}
		},
		new OperatorDef // ceiling
		{
			oper = "'",
			parameters = [typeof(float)],
			returnType = typeof(float),
			function = (args, _) =>
			{
				var item1 = (Optional<float>)args![0];

				if (!item1.HasValue)
				{
					return new Optional<float>();
				}

				return new Optional<float>(MathF.Ceiling(item1.Value));
			}
		},
		new OperatorDef
		{
			oper = "*",
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(float),
			function = (args, _) =>
			{
				var item1 = (Optional<float>)args![0];
				var item2 = (Optional<float>)args[1];

				if (!item1.HasValue || !item2.HasValue)
				{
					return new Optional<float>();
				}

				return new Optional<float>(item1.Value * item2.Value);
			}
		},
		new OperatorDef
		{
			oper = "/",
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(float),
			function = (args, _) =>
			{
				var item1 = (Optional<float>)args![0];
				var item2 = (Optional<float>)args[1];

				if (!item1.HasValue || !item2.HasValue)
				{
					return new Optional<float>();
				}

				return new Optional<float>(item2.Value / item1.Value);
			}
		},
		new OperatorDef
		{
			oper = "+",
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(float),
			function = (args, _) =>
			{
				var item1 = (Optional<float>)args![0];
				var item2 = (Optional<float>)args[1];

				if (!item1.HasValue || !item2.HasValue)
				{
					return new Optional<float>();
				}

				return new Optional<float>(item1.Value + item2.Value);
			}
		},
		new OperatorDef
		{
			oper = "-",
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(float),
			function = (args, _) =>
			{
				var item1 = (Optional<float>)args![0];
				var item2 = (Optional<float>)args[1];

				if (!item1.HasValue || !item2.HasValue)
				{
					return new Optional<float>();
				}

				return new Optional<float>(item2.Value - item1.Value);
			}
		},
		new OperatorDef
		{
			oper = "!=",
			parameters = [typeof(VariableObject), typeof(VariableObject)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				FunctionProcessor.ConvertUnknown(args![0], out var item1);
				FunctionProcessor.ConvertUnknown(args[1], out var item2);

				if (item1 == null || item2 == null)
				{
					return new Optional<bool>();
				}

				return new Optional<bool>(item1 != item2);
			}
		},
		new OperatorDef // checks string equivalence
		{
			oper = "==",
			parameters = [typeof(VariableObject), typeof(VariableObject)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				FunctionProcessor.ConvertUnknown(args![0], out var item1);
				FunctionProcessor.ConvertUnknown(args[1], out var item2);

				if (item1 == null || item2 == null)
				{
					return new Optional<bool>();
				}

				return new Optional<bool>(item1 == item2);
			}
		},
		new OperatorDef
		{
			oper = ">",
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = (Optional<float>)args![0];
				var item2 = (Optional<float>)args[1];

				if (!item1.HasValue || !item2.HasValue)
				{
					return new Optional<bool>();
				}

				return new Optional<bool>(item2.Value > item1.Value);
			}
		},
		new OperatorDef
		{
			oper = "<",
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = (Optional<float>)args![0];
				var item2 = (Optional<float>)args[1];

				if (!item1.HasValue || !item2.HasValue)
				{
					return new Optional<bool>();
				}

				return new Optional<bool>(item2.Value < item1.Value);
			}
		},
		new OperatorDef
		{
			oper = ">=",
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = (Optional<float>)args![0];
				var item2 = (Optional<float>)args[1];

				if (!item1.HasValue || !item2.HasValue)
				{
					return new Optional<bool>();
				}

				return new Optional<bool>(item2.Value >= item1.Value);
			}
		},
		new OperatorDef
		{
			oper = "<=",
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = (Optional<float>)args![0];
				var item2 = (Optional<float>)args[1];

				if (!item1.HasValue || !item2.HasValue)
				{
					return new Optional<bool>();
				}

				return new Optional<bool>(item2.Value <= item1.Value);
			}
		},
		new OperatorDef
		{
			oper = "&&",
			parameters = [typeof(bool), typeof(bool)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = (Optional<bool>)args![0];
				var item2 = (Optional<bool>)args[1];

				if (!item1.HasValue || !item2.HasValue)
				{
					return new Optional<bool>();
				}

				return new Optional<bool>(item1.Value && item2.Value);
			}
		},
		new OperatorDef
		{
			oper = "||",
			parameters = [typeof(bool), typeof(bool)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = (Optional<bool>)args![0];
				var item2 = (Optional<bool>)args[1];

				if (!item1.HasValue || !item2.HasValue)
				{
					return new Optional<bool>();
				}

				return new Optional<bool>(item1.Value || item2.Value);
			}
		},
		new OperatorDef
		{
			oper = "%",
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(float),
			function = (args, _) =>
			{
				var item1 = (Optional<float>)args![0];
				var item2 = (Optional<float>)args[1];

				if (!item1.HasValue || !item2.HasValue)
				{
					return new Optional<float>();
				}

				return new Optional<float>(item1.Value % item2.Value);
			}
		}
	];

	public static readonly Dictionary<string, OperatorDef> OperatorsFromString = [];
	public static string[]? Operators;
	public static Regex? OperatorRegex;

	public static void Initialize()
	{
		foreach (var oper in _operators)
		{
			OperatorsFromString.Add(oper.oper, oper);
		}

		Operators = OperatorsFromString.Keys.ToArray();
		HashSet<char> characters = new HashSet<char>();
		foreach (var oper in Operators)
		{
			characters.UnionWith(oper);
		}

		characters.Remove('-'); // '-' has to be at end b/c of regex stuff

		string regex_string = @$"[{string.Join("", from c in characters select $"{c}")}-]";
		Debug.WriteLine(regex_string);

		OperatorRegex = new Regex(regex_string, RegexOptions.Compiled | RegexOptions.NonBacktracking);
	}

	public static OperatorDef Get(string name)
	{
		return OperatorsFromString[name];
	}
}