using System;
using System.Collections.Generic;

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
				var item1 = FunctionProcessor.Convert<bool>(args[0]);

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
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(float),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float>(args[0]);
				var item2 = FunctionProcessor.Convert<float>(args[1]);

				if (!item1.HasValue || !item2.HasValue)
				{
					return new FunctionParameter<float?>(null);
				}

				return new FunctionParameter<float?>(Math.Pow(item2.Value, item1.Value));
			}
		},
		new OperatorDef
		{
			oper = "*",
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(float),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float>(args[0]);
				var item2 = FunctionProcessor.Convert<float>(args[1]);

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
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(float),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float>(args[0]);
				var item2 = FunctionProcessor.Convert<float>(args[1]);

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
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(float),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float>(args[0]);
				var item2 = FunctionProcessor.Convert<float>(args[1]);

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
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(float),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float>(args[0]);
				var item2 = FunctionProcessor.Convert<float>(args[1]);

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
			parameters = [typeof(object), typeof(object)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<object>(args[0]);
				var item2 = FunctionProcessor.Convert<object>(args[1]);

				if (item1 == null || item2 == null)
				{
					return new FunctionParameter<bool?>(null);
				}

				return new FunctionParameter<bool?>(item1.ToString() != item2.ToString());
			}
		},
		new OperatorDef // checks string equivalence
		{
			oper = "==",
			parameters = [typeof(VariableObject), typeof(VariableObject)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<object>(args[0]);
				var item2 = FunctionProcessor.Convert<object>(args[1]);

				if (item1 == null || item2 == null)
				{
					return new FunctionParameter<bool?>(null);
				}

				return new FunctionParameter<bool?>(item1.ToString() == item2.ToString());
			}
		},
		new OperatorDef
		{
			oper = ">",
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float>(args[0]);
				var item2 = FunctionProcessor.Convert<float>(args[1]);

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
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float>(args[0]);
				var item2 = FunctionProcessor.Convert<float>(args[1]);

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
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float>(args[0]);
				var item2 = FunctionProcessor.Convert<float>(args[1]);

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
			parameters = [typeof(float), typeof(float)],
			returnType = typeof(bool),
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float>(args[0]);
				var item2 = FunctionProcessor.Convert<float>(args[1]);

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

	public static Dictionary<string, OperatorDef> Operators;

	public static void Initialize()
	{
		Operators = [];

		foreach (var oper in _operators)
		{
			Operators.Add(oper.oper, oper);
		}
	}
}