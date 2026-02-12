using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using StoryIso.Debugging;
using StoryIso.Enums;
using StoryIso.Misc;

namespace StoryIso.Functions;

public static partial class ParameterEvaluator
{
	public static bool Evaluate<T>(Source source, (object, Type)[] postfix, out Optional<T> result) where T : notnull
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
				DebugConsole.Raise(new ParameterError(source, item.Item1.ToString() ?? "Type doesn't have a name", stack.Count, operatorDef.parameters.Length));
				result = default;
				return false;
			}

			List<object> parameters = [];

			for (int i = 0; i < operatorDef.parameters.Length; i++)
			{
				var param = stack.Pop();

				if (operatorDef.parameters[i] == typeof(float) && param.Item2 == typeof(int))
				{
					param = ((float)(int)param.Item1, typeof(float));
				}

				else if (operatorDef.parameters[i] == typeof(int) && param.Item2 == typeof(float))
				{
					param = ((int)(float)param.Item1, typeof(int));
				}

				else if (operatorDef.parameters[i] == typeof(string) && param.Item2 != typeof(string))
				{
					// cursed, but it works
					// I want to find a better way to do this...
					param = ((string)Convert.ChangeType(param.Item1, param.Item2), typeof(string));
				}

				else if (operatorDef.parameters[i] != typeof(VariableObject) && param.Item2 != operatorDef.parameters[i])
				{
					DebugConsole.Raise(new ParameterTypeError(source, operatorDef.oper, param.Item1.ToString() ?? "Type doesn't have a name", operatorDef.parameters[i].FullName ?? "Type doesn't have a name"));
					result = default;
					return false;
				}

				parameters.Add(param.Item1);
			}

			if (operatorDef.function == null)
			{
				throw new ArgumentNullException("function doesn't exist");
			}

			object? new_value = operatorDef.function(parameters, source);

			if (new_value == null)
			{
				result = default;
				return false;
			}

			stack.Push((new_value, operatorDef.returnType));
		}

		if (stack.Count != 1)
		{
			DebugConsole.Raise(new ParameterProcessError(source, "Too many parameters, not enough functions. "));
			result = default;
			return false;
		}

		var end_value = stack.Pop();

		if (!MiscFuncs.SimilarTypes(typeof(T), end_value.Item2))
		{
			if (typeof(T) == typeof(string) && end_value.Item2 != typeof(string))
			{
				result = new Optional<T>((T)(object)(FunctionProcessor.Convert<T>(end_value.Item1)?.ToString() ?? throw new InvalidOperationException("Value is null/doesn't convert to string")));
				return true;
			}

			if (typeof(T) == typeof(float) && end_value.Item2 == typeof(int)) // if return type is int and has float, convert
			{
				result = new Optional<T>((T)(object)(int)FunctionProcessor.Convert<float>(end_value.Item1));
				return true;
			}

			if (typeof(T) == typeof(int) && end_value.Item2 == typeof(float)) // if return type is int and has float, convert
			{
				result = new Optional<T>((T)(object)(float)FunctionProcessor.Convert<int>(end_value.Item1));
				return true;
			}

			DebugConsole.Raise(new ParameterProcessError(source));
			result = default;
			return false;	
		}

		result = new Optional<T>(FunctionProcessor.Convert<T>(end_value.Item1) ?? throw new InvalidOperationException("Value is null/doesn't convert to string"));
		return true;
	}

	private static int precedence(string oper)
	{
		return oper switch
		{
			"!" => 7,
			"^" => 6,
			"," or "'" => 5, // floor and ceiling operators
			"*" or "/" or "%" => 4,
			"+" or "-" => 3,
			"!=" or "==" or ">" or "=" or "<" or ">=" or "<=" => 2,
			"&&" => 1,
			"||" => 0,
			_ => -1,
		};
	}

	public static PostfixEquation<T>? Postfix<T>(Source source, string function, string value) where T : notnull
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