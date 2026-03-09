using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using StoryIso.Debugging;
using StoryIso.Misc;

namespace StoryIso.Functions;

public static partial class ParameterEvaluator
{
	private static readonly Regex _splitRegex = SplitRegex();
	private static readonly Regex _operandRegex = OperandRegex();

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

			if (operatorDef.function == null)
			{
				throw new ArgumentNullException($"function of operator {operatorDef.oper} doesn't exist");
			}

			if (stack.Count < operatorDef.parameters.Length)
			{
				DebugConsole.Raise(new ParameterError(source, item.Item1.ToString() ?? "Type doesn't have a name", stack.Count, operatorDef.parameters.Length));
				result = default;
				return false;
			}

			List<object> parameters = [];

			for (int i = operatorDef.parameters.Length - 1; i >= 0; i--)
			{
				var param = stack.Pop();

				if (!TryConvertParam(ref param, operatorDef.parameters[i], source, operatorDef.oper))
				{
					result = default;
					return false;
				}

				parameters.Add(param.Item1);
			}

			if (parameters.Count > 0)
			{
				parameters.Reverse();
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

		if (!TryConvertParam(ref end_value, typeof(T), source, "n\\a"))
		{
			result = default;
			return false;
		}

		var final = (Optional<T>)end_value.Item1;

		if (!final.HasValue)
		{
			throw new InvalidOperationException($"Value is null/doesn't convert to {typeof(T).Name}");
		}

		result = final;
		return true;
	}

	private static bool TryConvertParam(ref (object, Type) param, Type result_type, Source source, string oper)
	{
		if (param.Item2 == result_type)
		{
			return true;
		}

		if (param.Item2 == typeof(FunctionParameter<float>))
		{
			param = (ParameterProcessor.Convert<float>(param.Item1), typeof(float));
		}

		else if (param.Item2 == typeof(FunctionParameter<int>))
		{
			param = (ParameterProcessor.Convert<int>(param.Item1), typeof(int));
		}

		else if (param.Item2 == typeof(FunctionParameter<bool>))
		{
			param = (ParameterProcessor.Convert<bool>(param.Item1), typeof(bool));
		}

		else if (param.Item2 == typeof(FunctionParameter<string>))
		{
			var value = ParameterProcessor.Convert<string>(param.Item1);

			if (!value.HasValue)
			{
				DebugConsole.Raise(new ParameterTypeError(source, oper, param.Item2.Name ?? "Type doesn't have a name", result_type.FullName ?? "Type doesn't have a name"));
				return false;
			}

			param = (value, typeof(string));
		}

		if (result_type == typeof(float) && param.Item2 == typeof(int))
		{
			var param_value = (Optional<int>)param.Item1;

			if (!param_value.HasValue)
			{
				param = (new Optional<float>(), typeof(float));
				return true;
			}

			param = (new Optional<float>(param_value.Value), typeof(float));
			return true;
		}

		else if (result_type == typeof(int) && param.Item2 == typeof(float))
		{
			var param_value = (Optional<float>)param.Item1;

			if (!param_value.HasValue)
			{
				param = (new Optional<int>(), typeof(int));
				return true;
			}

			param = (new Optional<int>((int)param_value.Value), typeof(int));
			return true;
		}

		else if (result_type == typeof(string) && param.Item2 != typeof(string))
		{
			var param_value = ParameterProcessor.ConvertByTypeToString(param.Item1, param.Item2);

			if (param_value == null)
			{
				param = (new Optional<string>(), typeof(string));
				return true;
			}

			param = (new Optional<string>(param_value), typeof(string));
			return true;
		}

		else if (result_type != typeof(VariableObject) && param.Item2 != result_type)
		{
			DebugConsole.Raise(new ParameterTypeError(source, oper, param.Item2.Name ?? "Type doesn't have a name", result_type.FullName ?? "Type doesn't have a name"));
			return false;
		}

		return true;
	}

	private static int precedence(string oper)
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

	public static PostfixEquation<T>? Postfix<T>(Source source, string function, string value, string? obj) where T : notnull
	{
		string[] infix = (from match in
								_splitRegex.Matches(value)
								where !string.IsNullOrEmpty(match.Value)
								select match.Value).ToArray();

		// check the "equation" is just a single value
		if (infix.Length == 1 && _operandRegex.IsMatch(infix[0]) && !OperatorDefs.InlineFunctions.Contains(infix[0]))
		{
			var operand = ParameterProcessor.ProcessUnknownParameter(infix[0], source, function, obj);

			if (!operand.HasValue)
			{
				return null;
			}

			return new PostfixEquation<T>([operand.Value]);
		}

		List<(object, Type)> res = [];
		Stack<string> stack = new();

		bool previously_operand = false;

		for (int i = 0; i < infix.Length; i++)
		{
			string item = infix[i];

			if (!previously_operand && item == "-" && i != infix.Length - 1 && _operandRegex.IsMatch(infix[i + 1]))
			{
				var operand = ParameterProcessor.ProcessUnknownParameter(item + infix[i + 1], source, function, obj);
				i += 1;

				if (!operand.HasValue)
				{
					return null;
				}

				res.Add(operand.Value);
				previously_operand = true;
				continue;
			}

			if (_operandRegex.IsMatch(item) && !OperatorDefs.InlineFunctions.Contains(item))
			{
				var operand = ParameterProcessor.ProcessUnknownParameter(item, source, function, obj);

				if (!operand.HasValue)
				{
					return null;
				}

				res.Add(operand.Value);
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
				int parenthesis_depth = 0;

				for (int j = i + 2; j < infix.Length; j++)
				{
					if (infix[j] == ")")
					{
						parenthesis_depth -= 1;

						if (parenthesis_depth <= 0)
						{
							i = j + 1;
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
					var param_equation = Postfix<T1>(source, function, param, obj);

					if (param_equation == null)
					{
						return false;
					}

					res.Add((new FunctionParameter<T1>(param_equation), typeof(FunctionParameter<T1>)));
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

					if (oper.parameters[j] == typeof(bool))
					{
						if (!get_postfix<bool>(parameters[j]))
						{
							return null;
						}

						continue;
					}

					throw new NotImplementedException();
				}

				res.Add((oper, typeof(OperatorDef)));
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
			if (stack.Peek() == "(")
			{
				stack.Pop();
				continue;
			}

			res.Add((OperatorDefs.Get(stack.Pop()), typeof(OperatorDef)));
		}

		return new PostfixEquation<T>(res.ToArray());
	}

	[GeneratedRegex(@"("".+?"")|&&|\|\||==|!=|>=|<=|[()!<>+*/,-]|((?:(?!(&&|\|\||==|!=|>=|<=|[()""!><+*/ ,-]))).)+", RegexOptions.Compiled)]
	private static partial Regex SplitRegex();

	[GeneratedRegex(@"^("".+?"")|([-]?[A-Za-z0-9.]+)$", RegexOptions.Compiled)]
	private static partial Regex OperandRegex();
}