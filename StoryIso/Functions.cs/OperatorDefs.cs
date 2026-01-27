using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using StoryIso.Enums;

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
				var item1 = FunctionProcessor.Convert<bool?>(args[0]);

				if (!item1.HasValue)
				{
					return new FunctionParameter<bool?>();
				}

				return new FunctionParameter<bool?>(!item1.Value);
			}
		},
		new OperatorDef
		{
			oper = "^",
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(float),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float?>(args[0]);
				var item2 = FunctionProcessor.Convert<float?>(args[1]);

				if (!item1.HasValue || !item2.HasValue)
				{
					return new FunctionParameter<float?>();
				}

				return new FunctionParameter<float?>(MathF.Pow(item2.Value, item1.Value));
			}
		},
		new OperatorDef // floor
		{
			oper = ",",
			parameters = [typeof(float)],
			returnType = typeof(float),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float?>(args[0]);

				if (!item1.HasValue)
				{
					return new FunctionParameter<float?>();
				}

				return new FunctionParameter<float?>(MathF.Floor(item1.Value));
			}
		},
		new OperatorDef // ceiling
		{
			oper = "'",
			parameters = [typeof(float)],
			returnType = typeof(float),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float?>(args[0]);

				if (!item1.HasValue)
				{
					return new FunctionParameter<float?>();
				}

				return new FunctionParameter<float?>(MathF.Ceiling(item1.Value));
			}
		},
		new OperatorDef
		{
			oper = "*",
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(float),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float?>(args[0]);
				var item2 = FunctionProcessor.Convert<float?>(args[1]);

				if (!item1.HasValue || !item2.HasValue)
				{
					return new FunctionParameter<float?>();
				}

				return new FunctionParameter<float?>(item1.Value * item2.Value);
			}
		},
		new OperatorDef
		{
			oper = "/",
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(float),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float?>(args[0]);
				var item2 = FunctionProcessor.Convert<float?>(args[1]);

				if (!item1.HasValue || !item2.HasValue)
				{
					return new FunctionParameter<float?>();
				}

				return new FunctionParameter<float?>(item2.Value / item1.Value);
			}
		},
		new OperatorDef
		{
			oper = "+",
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(float),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float?>(args[0]);
				var item2 = FunctionProcessor.Convert<float?>(args[1]);

				if (!item1.HasValue || !item2.HasValue)
				{
					return new FunctionParameter<float?>();
				}

				return new FunctionParameter<float?>(item1.Value + item2.Value);
			}
		},
		new OperatorDef
		{
			oper = "-",
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(float),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float?>(args[0]);
				var item2 = FunctionProcessor.Convert<float?>(args[1]);

				if (!item1.HasValue || !item2.HasValue)
				{
					return new FunctionParameter<float?>();
				}

				return new FunctionParameter<float?>(item2.Value - item1.Value);
			}
		},
		new OperatorDef
		{
			oper = "!=",
			parameters = [typeof(VariableObject), typeof(VariableObject)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				FunctionProcessor.ConvertUnknown(args[0], out var item1);
				FunctionProcessor.ConvertUnknown(args[1], out var item2);

				if (item1 == null || item2 == null)
				{
					return new FunctionParameter<bool?>();
				}

				return new FunctionParameter<bool?>(item1 != item2);
			}
		},
		new OperatorDef // checks string equivalence
		{
			oper = "==",
			parameters = [typeof(VariableObject), typeof(VariableObject)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				FunctionProcessor.ConvertUnknown(args[0], out var item1);
				FunctionProcessor.ConvertUnknown(args[1], out var item2);

				if (item1 == null || item2 == null)
				{
					return new FunctionParameter<bool?>();
				}

				return new FunctionParameter<bool?>(item1 == item2);
			}
		},
		new OperatorDef
		{
			oper = ">",
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float?>(args[0]);
				var item2 = FunctionProcessor.Convert<float?>(args[1]);

				if (!item1.HasValue || !item2.HasValue)
				{
					return new FunctionParameter<bool?>();
				}

				return new FunctionParameter<bool?>(item2.Value > item1.Value);
			}
		},
		new OperatorDef
		{
			oper = "<",
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float?>(args[0]);
				var item2 = FunctionProcessor.Convert<float?>(args[1]);

				if (!item1.HasValue || !item2.HasValue)
				{
					return new FunctionParameter<bool?>();
				}

				return new FunctionParameter<bool?>(item2.Value < item1.Value);
			}
		},
		new OperatorDef
		{
			oper = ">=",
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float?>(args[0]);
				var item2 = FunctionProcessor.Convert<float?>(args[1]);

				if (!item1.HasValue || !item2.HasValue)
				{
					return new FunctionParameter<bool?>();
				}

				return new FunctionParameter<bool?>(item2.Value >= item1.Value);
			}
		},
		new OperatorDef
		{
			oper = "<=",
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float?>(args[0]);
				var item2 = FunctionProcessor.Convert<float?>(args[1]);

				if (!item1.HasValue || !item2.HasValue)
				{
					return new FunctionParameter<bool?>();
				}

				return new FunctionParameter<bool?>(item2.Value <= item1.Value);
			}
		},
		new OperatorDef
		{
			oper = "&&",
			parameters = [typeof(bool), typeof(bool)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = ((FunctionParameter<bool?>)args[0]).Value;
				var item2 = ((FunctionParameter<bool?>)args[1]).Value;

				if (!item1.HasValue || !item2.HasValue)
				{
					return new FunctionParameter<bool?>();
				}

				return new FunctionParameter<bool?>(item1.Value && item2.Value);
			}
		},
		new OperatorDef
		{
			oper = "||",
			parameters = [typeof(bool), typeof(bool)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = ((FunctionParameter<bool?>)args[0]).Value;
				var item2 = ((FunctionParameter<bool?>)args[1]).Value;

				if (!item1.HasValue || !item2.HasValue)
				{
					return new FunctionParameter<bool?>();
				}

				return new FunctionParameter<bool?>(item1.Value || item2.Value);
			}
		},
		new OperatorDef
		{
			oper = "%",
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(float),
			function = (args, _) =>
			{
				var item1 = ((FunctionParameter<float?>)args[0]).Value;
				var item2 = ((FunctionParameter<float?>)args[1]).Value;

				if (!item1.HasValue || !item2.HasValue)
				{
					return new FunctionParameter<float?>();
				}

				return new FunctionParameter<float?>(item1.Value % item2.Value);
			}
		}
	];

	public static readonly Dictionary<string, OperatorDef> OperatorsFromString = [];
	public static string[] Operators;
	public static Regex OperatorRegex;

	public static void Initialize()
	{
		foreach (var oper in _operators)
		{
			OperatorsFromString.Add(oper.oper, oper);
		}

		Operators = OperatorsFromString.Keys.ToArray();
		OperatorRegex = new Regex(@$"({string.Join('|', from oper in Operators select (from symbol in oper select $"[{symbol}]"))})", RegexOptions.Compiled);
	}

	public static OperatorDef Get(string name)
	{
		return OperatorsFromString[name];
	}
}