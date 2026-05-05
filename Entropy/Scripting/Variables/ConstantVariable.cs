using System;
using Entropy.Debugging;
using Entropy.Misc;

namespace Entropy.Scripting.Variables;

public struct ConstantVariable<T> : IVariable where T : notnull
{
	private readonly string _name;
	public readonly string Name => _name;

	public readonly Type ValueType => typeof(T);

	public readonly bool IsReadOnly => false;

	public readonly bool IsConstant => true;

	private readonly T _value;
	public readonly IOptional Value => new Optional<T>(_value);

	public ConstantVariable(string name, T value) 
	{
		this._name = name;
		this._value = value;
	}

	public readonly void Set(Source source, IOptional value)
	{
		DebugConsole.Raise(new ReadOnlyVariableError(source, this._name));
	}
}