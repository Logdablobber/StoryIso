using System;
using StoryIso.Debugging;
using StoryIso.Misc;

namespace StoryIso.Functions;

public struct FunctionParameter<T>
{
	private T _value;
	private string _variableName;
	public readonly T Value 
	{ 
		get
		{
			if (_value != null)
			{
				return _value;
			}

			if (_variableName == null)
			{
				return default;
			}

			if (typeof(T) == typeof(object))
			{
				if (VariableManager.ContainsVariable(_variableName, out var _, out object value))
				{
					return (T)value;
				}

				return default;
			}

			object variable_value = VariableManager.GetVariable<T>(_variableName, new Source(0, "GetVariable", "VariableManager"));

			return (T)variable_value;
		}
	}

	public FunctionParameter(T value, string variable_name = null)
	{
		_value = value;
		_variableName = variable_name;
	}

	public static explicit operator T(FunctionParameter<T> parameter)
	{
		return parameter.Value;
	} 
}