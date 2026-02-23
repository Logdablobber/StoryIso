using System;
using StoryIso.Debugging;
using StoryIso.Misc;

namespace StoryIso.Functions;

public struct FunctionParameter<T> where T : notnull
{
	private readonly T? _value;
	private readonly string? _variableName;
	private readonly PostfixEquation<T>? _postfixEquation;
	private readonly ReturnType _returnType;
	public readonly Optional<T> Value 
	{ 
		get
		{
			switch (_returnType)
			{
				case ReturnType.value:
					if (_value == null)
					{
						return default;
					}

					return _value;

				case ReturnType.variable:
					if (_variableName == null)
					{
						return default;
					}

					if (typeof(T) == typeof(object))
					{
						if (VariableManager.TryGetVariable(_variableName, out var _, out object? value))
						{
							if (value == null)
							{
								return default;
							}

							return (T)value;
						}

						return default;
					}

					object? variable_value = VariableManager.GetVariable<T>(_variableName, new Source(0, "GetVariable", "VariableManager"));

					if (variable_value == null)
					{
						return default;
					}

					return (T)variable_value;

				case ReturnType.equation:
					if (_postfixEquation == null || !_postfixEquation.Evaluate(new Source(0, "Evaluate Parameter", ""), out Optional<T> res))
					{
						return default;
					}

					return res;
			}

			return default;
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
		_returnType = ReturnType.value;
	}

	public FunctionParameter(string variable_name)
	{
		_value = default;
		_variableName = variable_name;
		_postfixEquation = null;
		_returnType = ReturnType.variable;
	}

	public FunctionParameter(PostfixEquation<T> equation)
	{
		_value = default;
		_variableName = null;
		_postfixEquation = equation;
		_returnType = ReturnType.equation;
	}

	public static explicit operator T?(FunctionParameter<T> parameter)
	{
		if (parameter.Value.HasValue)
		{
			return parameter.Value.Value;
		}

		return default;
	} 
}

enum ReturnType
{
	value = 1,
	variable = 2,
	equation = 3
} 