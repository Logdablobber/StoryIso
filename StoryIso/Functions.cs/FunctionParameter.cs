using System;
using StoryIso.Debugging;
using StoryIso.Misc;

namespace StoryIso.Functions;

public struct FunctionParameter<T> where T : notnull
{
	private readonly T? _value;
	private readonly string? _variableName;
	private readonly PostfixEquation<T>? _postfixEquation;
	public readonly T? Value 
	{ 
		get
		{
			if (_value != null)
			{
				return _value;
			}

			if (_variableName != null)
			{
				if (typeof(T) == typeof(object))
				{
					if (VariableManager.ContainsVariable(_variableName, out var _, out object? value))
					{
						return (T?)value;
					}

					return default;
				}

				object? variable_value = VariableManager.GetVariable<T>(_variableName, new Source(0, "GetVariable", "VariableManager"));

				return (T?)variable_value;
			}

			if (_postfixEquation == null || !_postfixEquation.Evaluate(new Source(0, "Evaluate Parameter", ""), out Optional<T> res))
			{
				return default;
			}

			return res.Value;
		}
	}

	public FunctionParameter()
	{
		_value = default;
		_variableName = null;
		_postfixEquation = null;
	}

	public FunctionParameter(T value)
	{
		_value = value;
		_variableName = null;
		_postfixEquation = null;
	}

	public FunctionParameter(string variable_name)
	{
		_value = default;
		_variableName = variable_name;
		_postfixEquation = null;
	}

	public FunctionParameter(PostfixEquation<T> equation)
	{
		_value = default;
		_variableName = null;
		_postfixEquation = equation;
	}

	public static explicit operator T?(FunctionParameter<T> parameter)
	{
		return parameter.Value;
	} 
}