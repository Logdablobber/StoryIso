using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using StoryIso.Debugging;
using StoryIso.Enums;

namespace StoryIso.Functions;

public static partial class ParameterEvaluator
{
	public static bool Evaluate<T>(Source source, (object, Type)[] postfix, out T result)
	{
		Stack<(object, Type)> stack = new();

		foreach (var item in postfix)
		{
			if (item.Item2 != typeof(OperatorDef))
			{
				stack.Push(item);
				continue;
			}

			var operatorDef = (OperatorDef)item.Item1;

			if (stack.Count < operatorDef.parameters.Length)
			{
				DebugConsole.Raise(new ParameterError(source, item.Item1.ToString(), stack.Count, operatorDef.parameters.Length));
				result = default;
				return false;
			}

			List<object> parameters = [];

			for (int i = 0; i < operatorDef.parameters.Length; i++)
			{
				var param = stack.Pop();

				if (operatorDef.parameters[i] != typeof(VariableObject) && param.Item2 != operatorDef.parameters[i])
				{
					DebugConsole.Raise(new ParameterTypeError(source, operatorDef.oper, param.Item1.ToString(), operatorDef.parameters[i].FullName));
					result = default;
					return false;
				}

				parameters.Add(param.Item1);
			}

			object new_value = operatorDef.function(parameters, source);

			stack.Push((new_value, operatorDef.returnType));
		}

		if (stack.Count != 1)
		{
			DebugConsole.Raise(new ParameterProcessError(source, "Too many parameters, not enough functions. "));
			result = default;
			return false;
		}

		var end_value = stack.Pop();

		var type = typeof(T);

		if (!(end_value.Item2 == typeof(T) || typeof(T).FullName.Contains(end_value.Item2.FullName))) // second part is for nullables
		{
			DebugConsole.Raise(new ParameterProcessError(source));
			result = default;
			return false;	
		}

		result = FunctionProcessor.Convert<T>(end_value.Item1);
		return true;
	}

	private static int precedence(string oper)
	{
		return oper switch
		{
			"!" => 6,
			"^" => 5,
			"*" or "/" => 4,
			"+" or "-" => 3,
			"!=" or "==" or ">" or "=" or "<" or ">=" or "<=" => 2,
			"&&" => 1,
			"||" => 0,
			_ => -1,
		};
	}

	public static PostfixEquation<T> Postfix<T>(Source source, string function, string value)
	{
		string[] infix = (from match in
								SplitRegex().Matches(value)
								select match.Value).ToArray();

		List<(object, Type)> res = [];
		Stack<string> stack = new();

		foreach (string item in infix)
		{
			if (string.IsNullOrEmpty(item))
			{
				continue;
			}

			if (OperandRegex().IsMatch(item))
			{
				var operand = ParameterProcessor.ProcessUnknownParameter(item, source, function);

				if (!operand.HasValue)
				{
					return null;
				}

				res.Add(operand.Value);
				continue;
			}

			if (item == "(")
			{
				stack.Push("(");
				continue;
			}

			if (item == ")")
			{
				while (stack.Count > 0 && stack.Peek() != "(")
				{
					res.Add((OperatorDefs.Get(stack.Pop()), typeof(OperatorDef)));
				}

				stack.Pop();
				continue;
			}

			while (stack.Count > 0 && stack.Peek() != "(" &&
					(precedence(stack.Peek()) > precedence(item) || 
					(precedence(stack.Peek()) == precedence(item) &&
					item != "^")))
			{
				res.Add((OperatorDefs.Get(stack.Pop()), typeof(OperatorDef)));
			}

			stack.Push(item);
		}

		while (stack.Count > 0)
		{
			res.Add((OperatorDefs.Get(stack.Pop()), typeof(OperatorDef)));
		}

		return new PostfixEquation<T>(res.ToArray());
	}

	[GeneratedRegex(@"("".+?"")|&&|\|\||==|!=|>=|<=|[()!<>+*/-]|((?:(?!(&&|\|\||==|!=|>=|<=|[()""!><+*/ -]))).)+", RegexOptions.Compiled)]
	private static partial Regex SplitRegex();

	[GeneratedRegex(@"^("".+?"")|([A-Za-z0-9.]+)$", RegexOptions.Compiled)]
	private static partial Regex OperandRegex();
}