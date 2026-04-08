using System;
using System.Diagnostics;
using StoryIso.Debugging;
using StoryIso.Enums;
using StoryIso.Misc;
using StoryIso.Scripting.Variables;

namespace StoryIso.Scripting;

public struct FunctionParameter<T> : IFunctionParameter where T : notnull
{
	private readonly Scope? _scope;

	public readonly bool IsConstant { get; }
	public readonly Type ValueType
	{
		get
		{
			return typeof(T);
		}
	}

	public readonly VariableType variableType
	{
		get
		{
			return VariableManager.GetVariableType(typeof(T));
		}
	}

	private readonly T? _value;
	private readonly string? _variableName;
	private readonly EquationTree<T>? _equation;
	private readonly ReturnType _returnType;
	public readonly Optional<T> GetValue(Source source)
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

				var value = _scope?.GetVariable(source, _variableName, variableType, out _);

				if (value == null || !value.HasValue)
				{
					return default;
				}

				return (Optional<T>)value;

			case ReturnType.equation:
				if (_equation == null)
				{
					return default;
				}

				return _equation.Evaluate(new Source(0, "Evaluate Parameter", ""));
		}

		return default;
	}

	public FunctionParameter()
	{
		_value = default;
		_variableName = null;
		_equation = null;
		IsConstant = false;
	}

	public FunctionParameter(T value)
	{
		_value = value;
		_variableName = null;
		_equation = null;
		_returnType = ReturnType.value;
		IsConstant = true;
	}

	public FunctionParameter(Source source, Scope scope, string variable_name)
	{
		var variable = scope.GetVariable(source, variable_name, VariableManager.GetVariableType(typeof(T)), out bool is_constant);
		
		if (variable.HasValue && is_constant)
		{
			if (variable is not ConstantVariable<T> value)
			{
				return;
			}

			_variableName = null;
			_returnType = ReturnType.value;
			IsConstant = true;

			_value = ((Optional<T>)value.Value).Value;
			return;
		}

		_value = default;
		_variableName = variable_name;
		_equation = null;
		_returnType = ReturnType.variable;
		_scope = scope;
	}

	public FunctionParameter(EquationTree<T> equation, Source source)
	{
		if (equation.IsConstant)
		{
			var value = equation.Evaluate(source);

			if (!value.HasValue)
			{
				throw new NullReferenceException("Equation failed to evaluate");
			}

			_value = value.Value;
			_variableName = null;
			_equation = null;
			_returnType = ReturnType.value;
			return;
		}

		_value = default;
		_variableName = null;
		_equation = equation;
		_returnType = ReturnType.equation;
	}

	public FunctionParameter<T1>? ConvertTo<T1>(Source source, Scope scope) where T1 : notnull
	{
		if (typeof(T) == typeof(T1))
		{
			return (FunctionParameter<T1>)(object)this;
		}

		if (this._returnType == ReturnType.value)
		{
			Optional<T1> new_value = ParameterProcessor.ConvertParam<T1>(source, this);

			if (!new_value.HasValue)
			{
				throw new NullReferenceException("value is null");
			}

			return new FunctionParameter<T1>(value:new_value.Value);
		}

		if (this._returnType == ReturnType.variable)
		{
			if (this._variableName == null || this._scope == null)
			{
				throw new NullReferenceException();
			}

			return new FunctionParameter<T1>(source, _scope, this._variableName);
		}

		if (this._returnType == ReturnType.equation)
		{
			if (this._equation == null)
			{
				throw new NullReferenceException();
			}

			return new FunctionParameter<T1>(this._equation.ConvertTo<T1>(), source);
		}

		throw new NotImplementedException();
	}
}

enum ReturnType
{
	value = 1,
	variable = 2,
	equation = 3
} 