using System;
using Entropy.Debugging;
using Entropy.Enums;
using Entropy.Misc;
using Entropy.Scripting.Variables;

namespace Entropy.Scripting;

public readonly struct FunctionParameter<T> : IFunctionParameter where T : notnull
{
	private readonly Scope? _scope;

	public bool IsConstant { get; }
	public Type ValueType => typeof(T);

	public VariableType variableType => VariableManager.GetVariableType(typeof(T));

	private readonly T? _value;
	private readonly string? _variableName;
	private readonly EquationTree<T>? _equation;
	private readonly ReturnType _returnType;
	public Optional<T> GetValue(Source source)
	{ 
		switch (_returnType)
		{
			case ReturnType.Value:
				if (_value == null)
				{
					return default;
				}

				return _value;

			case ReturnType.Variable:
				if (_variableName == null)
				{
					return default;
				}

				var value = _scope?.GetVariable(source, _variableName, variableType, out _);

				if (value is not { HasValue: true })
				{
					return default;
				}

				return (Optional<T>)value;

			case ReturnType.Equation:

				return _equation?.Evaluate(new Source(0, "Evaluate Parameter", "")) ?? default;
            
			default:
				return new Optional<T>();
		}
	}

	public FunctionParameter()
	{
		_value = default;
		_variableName = null;
		_equation = null;
		_returnType = ReturnType.None;
		IsConstant = false;
	}

	public FunctionParameter(T value)
	{
		_value = value;
		_variableName = null;
		_equation = null;
		_returnType = ReturnType.Value;
		IsConstant = true;
	}

	public FunctionParameter(Source source, Scope scope, string variable_name)
	{
		var variable = scope.GetVariable(source, variable_name, VariableManager.GetVariableType(typeof(T)), out bool is_constant);
		
		if (variable.HasValue && is_constant)
		{
			if (variable is not Optional<T> value)
			{
				return;
			}

			_variableName = null;
			_returnType = ReturnType.Value;
			IsConstant = true;

			_value = value.HasValue ? value.Value : default;
			return;
		}

		_value = default;
		_variableName = variable_name;
		_equation = null;
		_returnType = ReturnType.Variable;
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
			_returnType = ReturnType.Value;
			return;
		}

		_value = default;
		_variableName = null;
		_equation = equation;
		_returnType = ReturnType.Equation;
	}

	public FunctionParameter<T1>? ConvertTo<T1>(Source source, Scope scope) where T1 : notnull
	{
		if (typeof(T) == typeof(T1))
		{
			return (FunctionParameter<T1>)(object)this;
		}

		switch (this._returnType)
		{
			case ReturnType.Value:
			{
				Optional<T1> new_value = ParameterProcessor.ConvertParam<T1>(source, this);

				if (!new_value.HasValue)
				{
					throw new NullReferenceException("value is null");
				}

				return new FunctionParameter<T1>(value:new_value.Value);
			}
            
			case ReturnType.Variable when this._variableName == null || this._scope == null:
				throw new NullReferenceException();
            
			case ReturnType.Variable:
				return new FunctionParameter<T1>(source, _scope, this._variableName);
            
			case ReturnType.Equation when this._equation == null:
				throw new NullReferenceException();
            
			case ReturnType.Equation:
				return new FunctionParameter<T1>(this._equation.ConvertTo<T1>(), source);
            
			default:
				throw new NotImplementedException();
		}
	}
}

enum ReturnType
{
    None = 0,
	Value = 1,
	Variable = 2,
	Equation = 3
} 