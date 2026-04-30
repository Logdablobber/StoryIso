using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SharpDX;
using StoryIso.Debugging;
using StoryIso.Misc;
namespace StoryIso.Scripting;

public static partial class ParameterEvaluator
{
	private static readonly Regex _splitRegex = SplitRegex();
	private static readonly Regex _operandRegex = OperandRegex();

	public static bool ToNodeTree<T>(Source source, Scope scope, string function, string infix, out FunctionParameter<T>? result) where T : notnull
	{
		var postfix = Postfix(source, scope, function, infix);

		if (postfix == null)
		{
			result = null;
			return false;
		}

		Stack<IFunctionParameter> stack = new();

		foreach (var item in postfix)
		{
			if (item.Item2 != typeof(OperatorDef))
			{
				stack.Push((IFunctionParameter)item.Item1);
				continue;
			}

			var operatorDef = (OperatorDef)item.Item1;

			if (operatorDef.function == null)
			{
				throw new ArgumentNullException($"function of operator {operatorDef.oper} doesn't exist");
			}

			if (stack.Count < operatorDef.parameters.Length)
			{
				DebugConsole.Raise(new ParameterError(source, operatorDef.oper, stack.Count, operatorDef.parameters.Length));
				result = null;
				return false;
			}

			List<IFunctionParameter> parameters = [];

			for (int i = operatorDef.parameters.Length - 1; i >= 0; i--)
			{
				parameters.Add(stack.Pop());
			}

			if (parameters.Count > 0)
			{
				parameters.Reverse();
			}

			var new_param = IFunctionParameter.Create(parameters.ToArray(), operatorDef, source);
            
			stack.Push(new_param);
		}

		if (stack.Count != 1)
		{
			DebugConsole.Raise(new ParameterProcessError(source, "Too many parameters, not enough functions. "));
			result = null;
			return false;
		}

		result = stack.Pop().ConvertTo<T>(source, scope);
		return true;
	}

	private static int Precedence(string oper)
	{
		if (OperatorDefs.InlineFunctions.Contains(oper))
		{
			return 7;
		}

		return oper switch
		{
			"!" => 6,
			"^" => 5,
			"*" or "/" or "%" => 4,
			"+" or "-" => 3,
			"!=" or "==" or ">" or "=" or "<" or ">=" or "<=" => 2,
			"&&" => 1,
			"||" => 0,
			_ => -1,
		};
	}
	private static (object, Type)[]? Postfix(Source source, Scope scope, string function, string value)
	{
		string[] infix = (from match in
								_splitRegex.Matches(value)
								where !string.IsNullOrEmpty(match.Value)
								select match.Value).ToArray();

		// check the "equation" is just a single value
		if (infix.Length == 1 && _operandRegex.IsMatch(infix[0]) && !OperatorDefs.InlineFunctions.Contains(infix[0]))
		{
			var operand = ParameterProcessor.ProcessUnknownParameter(scope, infix[0], source, function);

			if (operand == null)
			{
				return null;
			}

			return [(operand, typeof(IFunctionParameter))];
		}

		List<(object, Type)> res = [];
		Stack<string> stack = new();

		bool previously_operand = false;

		for (int i = 0; i < infix.Length; i++)
		{
			string item = infix[i];

			if (!previously_operand && item == "-" && i != infix.Length - 1 && _operandRegex.IsMatch(infix[i + 1]))
			{
				var operand = ParameterProcessor.ProcessUnknownParameter(scope, item + infix[i + 1], source, function);
				i += 1;

				if (operand == null)
				{
					return null;
				}

				res.Add((operand, typeof(IFunctionParameter)));
				previously_operand = true;
				continue;
			}

			if (_operandRegex.IsMatch(item) && !OperatorDefs.InlineFunctions.Contains(item))
			{
				var operand = ParameterProcessor.ProcessUnknownParameter(scope, item, source, function);

				if (operand == null)
				{
					return null;
				}

				res.Add((operand, typeof(IFunctionParameter)));
				previously_operand = true;
				continue;
			}

			previously_operand = false;

			// parse inline functions:
			if (OperatorDefs.InlineFunctions.Contains(item))
			{
				if (i >= infix.Length - 1 || infix[i + 1] != "(")
				{
					DebugConsole.Raise(new MissingParenthesisError(source, value, "Missing open parenthesis"));
					return null;
				}

				var oper = OperatorDefs.Get(item);

				List<string> parameters = [""];
				var parenthesis_depth = 0;

				for (int j = i + 2; j < infix.Length; j++)
				{
					if (infix[j] == ")")
					{
						parenthesis_depth -= 1;

						if (parenthesis_depth < 0)
						{
							i = j;
							break;
						}
						
						parameters[^1] += ")";
						continue;
					}

					if (j == infix.Length - 1)
					{
						DebugConsole.Raise(new MissingParenthesisError(source, value, "Missing closing parenthesis"));
						return null;
					}

					if (infix[j] == "(")
					{
						parenthesis_depth += 1;
						parameters[^1] += "(";
						continue;
					}

					if (parenthesis_depth == 0 && infix[j] == ",")
					{
						parameters.Add("");
						continue;
					}

					parameters[^1] += infix[j];
				}

				if (parameters.Count != oper.parameters.Length)
				{
					DebugConsole.Raise(new ParameterError(source, function, parameters.Count, oper.parameters.Length));
					return null;
				}

				bool get_postfix<T1>(string param) where T1 : notnull
				{
					if (!ToNodeTree<T1>(source, scope, function, param, out var equation))
					{
						return false;
					}

					res.Add((equation!, typeof(IFunctionParameter)));
					return true;
				}

				for (int j = 0; j < oper.parameters.Length; j++)
				{
					if (oper.parameters[j] == typeof(int))
					{
						if (!get_postfix<int>(parameters[j]))
						{
							return null;
						}

						continue;
					}

					if (oper.parameters[j] == typeof(float))
					{
						if (!get_postfix<float>(parameters[j]))
						{
							return null;
						}

						continue;
					}

					if (oper.parameters[j] == typeof(string))
					{
						if (!get_postfix<string>(parameters[j]))
						{
							return null;
						}

						continue;
					}

					if (oper.parameters[j] != typeof(bool))
					{
						throw new NotImplementedException();
					}

					if (!get_postfix<bool>(parameters[j]))
					{
						return null;
					}
				}

				res.Add((oper, typeof(OperatorDef)));
				continue;
			}

			switch (item)
			{
				case "(":
					stack.Push("(");
					continue;
                
				case ")":
				{
					while (stack.Count > 0 && stack.Peek() != "(")
					{
						res.Add((OperatorDefs.Get(stack.Pop()), typeof(OperatorDef)));
					}

					stack.Pop();
					continue;
				}
			}

			while (stack.Count > 0 && stack.Peek() != "(" &&
			       (Precedence(stack.Peek()) > Precedence(item) || 
			        (Precedence(stack.Peek()) == Precedence(item) &&
			         item != "^")))
			{
				res.Add((OperatorDefs.Get(stack.Pop()), typeof(OperatorDef)));
			}

			stack.Push(item);
		}

		while (stack.Count > 0)
		{
			if (stack.Peek() == "(")
			{
				stack.Pop();
				continue;
			}

			res.Add((OperatorDefs.Get(stack.Pop()), typeof(OperatorDef)));
		}

		return res.ToArray();
	}

	[GeneratedRegex(@"("".+?"")|&&|\|\||==|!=|>=|<=|[()!<>+*/,-]|((?:(?!(&&|\|\||==|!=|>=|<=|[()""!><+*/ ,-]))).)+", RegexOptions.Compiled)]
	private static partial Regex SplitRegex();

	[GeneratedRegex(@"^("".+?"")|([-]?[A-Za-z0-9.]+)$", RegexOptions.Compiled)]
	private static partial Regex OperandRegex();
}