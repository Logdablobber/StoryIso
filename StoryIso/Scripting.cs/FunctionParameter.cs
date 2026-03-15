using System;
using System.Diagnostics;
using StoryIso.Debugging;
using StoryIso.Misc;

namespace StoryIso.Scripting;

public struct FunctionParameter<T> : IFunctionParameter where T : notnull
{
	public readonly bool IsConstant { get; }
	public readonly Type ValueType
	{
		get
		{
			return typeof(T);
		}
	}

	private readonly T? _value;
	private readonly string? _variableName;
	private readonly EquationTree<T>? _equation;
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
					if (_equation == null)
					{
						return default;
					}

					return _equation.Evaluate(new Source(0, "Evaluate Parameter", ""));
			}

			return default;
		}
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

	public FunctionParameter(string variable_name)
	{
		_value = default;
		_variableName = variable_name;
		_equation = null;
		_returnType = ReturnType.variable;
		
		if (VariableManager.IsVariableConstant(variable_name, out var value, out var type))
		{
			if (value == null)
			{
				return;
			}

			_variableName = null;
			_returnType = ReturnType.value;
			IsConstant = true;

			if (typeof(T) == type)
			{
				_value = (T)value;
				return;
			}

			if (typeof(T) == typeof(string))
			{
				if (type == typeof(int))
				{
					_value = (T)(object)((int)value).ToString();
					return;
				}

				if (type == typeof(float))
				{
					_value = (T)(object)((float)value).ToString();
					return;
				}

				if (type == typeof(bool))
				{
					_value = (T)(object)((bool)value).ToString();
					return;
				}

				throw new NotImplementedException();
			}

			if (typeof(T) == typeof(int))
			{
				if (type == typeof(float))
				{
					_value = (T)(object)(int)(float)value;
					return;
				}
				
				if (type == typeof(string) && int.TryParse((string)value, out int int_value))
				{
					_value = (T)(object)int_value;
					return;
				}

				throw new NotImplementedException();
			}

			if (typeof(T) == typeof(float))
			{
				if (type == typeof(int))
				{
					_value = (T)(object)(float)(int)value;
					return;
				}
				
				if (type == typeof(string) && float.TryParse((string)value, out float float_value))
				{
					_value = (T)(object)float_value;
					return;
				}

				throw new NotImplementedException();
			}

			if (typeof(T) == typeof(bool))
			{
				if (type == typeof(string) && bool.TryParse((string)value, out bool bool_value))
				{
					_value = (T)(object)bool_value;
					return;
				}

				throw new NotImplementedException();
			}

			throw new NotImplementedException();
		}
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

	public static explicit operator T?(FunctionParameter<T> parameter)
	{
		if (parameter.Value.HasValue)
		{
			return parameter.Value.Value;
		}

		return default;
	} 	

	public FunctionParameter<T1>? ConvertTo<T1>(Source source) where T1 : notnull
	{
		if (typeof(T) == typeof(T1))
		{
			return (FunctionParameter<T1>)(object)this;
		}

		if (this._returnType == ReturnType.value)
		{
			if (typeof(T1) == typeof(string))
			{
				var string_value = ParameterProcessor.ConvertByTypeToString(this) ?? throw new UnreachableException();

				return new FunctionParameter<T1>(value:(T1)(object)string_value);
			}

			// float to int
			if (typeof(T) == typeof(float) && typeof(T1) == typeof(int))
			{
				return new FunctionParameter<T1>((T1)(object)(int)(float)(object)this.Value.Value);
			}

			// int to float
			if (typeof(T) == typeof(int) && typeof(T1) == typeof(float))
			{
				return new FunctionParameter<T1>((T1)(object)(float)(int)(object)this.Value.Value);
			}

			throw new NotImplementedException();
		}

		if (this._returnType == ReturnType.variable)
		{
			if (this._variableName == null)
			{
				throw new NullReferenceException();
			}

			return new FunctionParameter<T1>(this._variableName);
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