using System;
using System.Collections.Generic;
using System.Text;
using StoryIso.Enums;

namespace StoryIso.Functions;

public static class OperatorDefs
{
	static readonly OperatorDef[] _operators = 
	[
		new OperatorDef
		{
			oper = "!",
			type = OperatorType.Not,
			parameters = [typeof(bool)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<bool?>(args[0]);

				if (!item1.HasValue)
				{
					return new FunctionParameter<bool?>(null);
				}

				return new FunctionParameter<bool?>(!item1.Value);
			}
		},
		new OperatorDef
		{
			oper = "^",
			type = OperatorType.Power,
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(float),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float?>(args[0]);
				var item2 = FunctionProcessor.Convert<float?>(args[1]);

				if (!item1.HasValue || !item2.HasValue)
				{
					return new FunctionParameter<float?>(null);
				}

				return new FunctionParameter<float?>(MathF.Pow(item2.Value, item1.Value));
			}
		},
		new OperatorDef
		{
			oper = "*",
			type = OperatorType.Times,
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(float),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float?>(args[0]);
				var item2 = FunctionProcessor.Convert<float?>(args[1]);

				if (!item1.HasValue || !item2.HasValue)
				{
					return new FunctionParameter<float?>(null);
				}

				return new FunctionParameter<float?>(item1.Value * item2.Value);
			}
		},
		new OperatorDef
		{
			oper = "/",
			type = OperatorType.Divide,
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(float),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float?>(args[0]);
				var item2 = FunctionProcessor.Convert<float?>(args[1]);

				if (!item1.HasValue || !item2.HasValue)
				{
					return new FunctionParameter<float?>(null);
				}

				return new FunctionParameter<float?>(item2.Value / item1.Value);
			}
		},
		new OperatorDef
		{
			oper = "+",
			type = OperatorType.Plus,
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(float),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float?>(args[0]);
				var item2 = FunctionProcessor.Convert<float?>(args[1]);

				if (!item1.HasValue || !item2.HasValue)
				{
					return new FunctionParameter<float?>(null);
				}

				return new FunctionParameter<float?>(item1.Value + item2.Value);
			}
		},
		new OperatorDef
		{
			oper = "-",
			type = OperatorType.Minus,
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(float),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float?>(args[0]);
				var item2 = FunctionProcessor.Convert<float?>(args[1]);

				if (!item1.HasValue || !item2.HasValue)
				{
					return new FunctionParameter<float?>(null);
				}

				return new FunctionParameter<float?>(item2.Value - item1.Value);
			}
		},
		new OperatorDef
		{
			oper = "!=",
			type = OperatorType.NotEqual,
			parameters = [typeof(VariableObject), typeof(VariableObject)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.ConvertBase(args[0]);
				var item2 = FunctionProcessor.ConvertBase(args[1]);

				if (item1 == null || item2 == null)
				{
					return new FunctionParameter<bool?>(null);
				}

				return new FunctionParameter<bool?>(item1 != item2);
			}
		},
		new OperatorDef // checks string equivalence
		{
			oper = "==",
			type = OperatorType.Equal,
			parameters = [typeof(VariableObject), typeof(VariableObject)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.ConvertBase(args[0]);
				var item2 = FunctionProcessor.ConvertBase(args[1]);

				if (item1 == null || item2 == null)
				{
					return new FunctionParameter<bool?>(null);
				}

				return new FunctionParameter<bool?>(item1 == item2);
			}
		},
		new OperatorDef
		{
			oper = ">",
			type = OperatorType.GreaterThan,
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float?>(args[0]);
				var item2 = FunctionProcessor.Convert<float?>(args[1]);

				if (!item1.HasValue || !item2.HasValue)
				{
					return new FunctionParameter<bool?>(null);
				}

				return new FunctionParameter<bool?>(item2.Value > item1.Value);
			}
		},
		new OperatorDef
		{
			oper = "<",
			type = OperatorType.LessThan,
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float?>(args[0]);
				var item2 = FunctionProcessor.Convert<float?>(args[1]);

				if (!item1.HasValue || !item2.HasValue)
				{
					return new FunctionParameter<bool?>(null);
				}

				return new FunctionParameter<bool?>(item2.Value < item1.Value);
			}
		},
		new OperatorDef
		{
			oper = ">=",
			type = OperatorType.GreaterThanOrEqual,
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float?>(args[0]);
				var item2 = FunctionProcessor.Convert<float?>(args[1]);

				if (!item1.HasValue || !item2.HasValue)
				{
					return new FunctionParameter<bool?>(null);
				}

				return new FunctionParameter<bool?>(item2.Value >= item1.Value);
			}
		},
		new OperatorDef
		{
			oper = "<=",
			type = OperatorType.LessThanOrEqual,
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float?>(args[0]);
				var item2 = FunctionProcessor.Convert<float?>(args[1]);

				if (!item1.HasValue || !item2.HasValue)
				{
					return new FunctionParameter<bool?>(null);
				}

				return new FunctionParameter<bool?>(item2.Value <= item1.Value);
			}
		},
		new OperatorDef
		{
			oper = "&&",
			type = OperatorType.And,
			parameters = [typeof(bool), typeof(bool)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = ((FunctionParameter<bool?>)args[0]).Value;
				var item2 = ((FunctionParameter<bool?>)args[1]).Value;

				if (!item1.HasValue || !item2.HasValue)
				{
					return new FunctionParameter<bool?>(null);
				}

				return new FunctionParameter<bool?>(item1.Value && item2.Value);
			}
		},
		new OperatorDef
		{
			oper = "||",
			type = OperatorType.Or,
			parameters = [typeof(bool), typeof(bool)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = ((FunctionParameter<bool?>)args[0]).Value;
				var item2 = ((FunctionParameter<bool?>)args[1]).Value;

				if (!item1.HasValue || !item2.HasValue)
				{
					return new FunctionParameter<bool?>(null);
				}

				return new FunctionParameter<bool?>(item1.Value || item2.Value);
			}
		}
	];
	// TODO: Implement modulo

	public static Dictionary<string, OperatorDef> OperatorsFromString;
	public static Dictionary<OperatorType, OperatorDef> OperatorsFromType;

	public static void Initialize()
	{
		OperatorsFromString = [];
		OperatorsFromType = [];

		foreach (var oper in _operators)
		{
			OperatorsFromString.Add(oper.oper, oper);
			OperatorsFromType.Add(oper.type, oper);
		}
	}

	public static OperatorDef Get(string name)
	{
		return OperatorsFromString[name];
	}

	public static OperatorDef Get(OperatorType type)
	{
		return OperatorsFromType[type];
	}
}