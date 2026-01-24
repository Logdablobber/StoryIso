using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using StoryIso.Debugging;

namespace StoryIso.Functions;

public static partial class ParameterEvaluator
{
	public static bool Evaluate<T>(Source source, (object, Type)[] postfix, out T result) where T : IParsable<T>
	{
		Stack<(object, Type)> stack = new();

		foreach (var item in postfix)
		{
			if (item.Item2 != typeof(OperatorDef))
			{
				stack.Push(item);
				continue;
			}

			/*if ((postfix.Item1 as OperatorDef).)
			{
				DebugConsole.Raise(new UnknownFunctionError(source, item));
				return false;
			}*/

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

				if (param.Item2 != operatorDef.parameters[i])
				{
					DebugConsole.Raise(new ParameterTypeError(source, operatorDef.oper, param.Item1.ToString(), param.Item2.FullName));
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

		if (end_value.Item2 != typeof(T))
		{
			DebugConsole.Raise(new ParameterProcessError(source));
			result = default;
			return false;	
		}

		result = (T)end_value.Item1;
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
		Stack<string> stack = new Stack<string>();

		foreach (string item in infix)
		{
			if (string.IsNullOrEmpty(item))
			{
				continue;
			}

			if (OperandRegex().IsMatch(item))
			{
				res.Add(ParameterProcessor.ParseParameterVariable(item, ));
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
					res.Add(stack.Pop());
				}

				stack.Pop();
				continue;
			}

			while (stack.Count > 0 && stack.Peek() != "(" &&
					(precedence(stack.Peek()) > precedence(item) || 
					(precedence(stack.Peek()) == precedence(item) &&
					item != "^")))
			{
				res.Add(stack.Pop());
			}

			stack.Push(item);
		}

		while (stack.Count > 0)
		{
			res.Add(stack.Pop());
		}

		// TODO: MAKE THIS RETURN (object, Type)

		return new PostfixEquation<T>(res.ToArray());
	}

	[GeneratedRegex(@"("".+?"")|&&|\|\||==|!=|>=|<=|[()!<>+*/-]|((?:(?!(&&|\|\||==|!=|>=|<=|[()""!><+*/ -]))).)+", RegexOptions.Compiled)]
	private static partial Regex SplitRegex();

	[GeneratedRegex(@"^("".+?"")|([A-Za-z0-9.]+)$", RegexOptions.Compiled)]
	private static partial Regex OperandRegex();
}