using System;
using System.Diagnostics;
using Entropy.Debugging;
using Entropy.Misc;

namespace Entropy.Scripting.Variables;

public struct ValueVariable<T> : IVariable where T : notnull
{
	private readonly string _name;
	public string Name => _name;

	public Type ValueType => typeof(T);

	public bool IsReadOnly => false;

	public bool IsConstant => false;

	private Optional<T> _value;
	public IOptional Value => _value;

	public ValueVariable(string name, Optional<T> value) 
	{
		this._name = name;
		this._value = value;
	}

	public void Set(Source source, IOptional value)
	{
		if (value is Optional<T> true_value)
		{
			this._value = true_value;
			return;
		}

		throw new UnreachableException();
	}
}