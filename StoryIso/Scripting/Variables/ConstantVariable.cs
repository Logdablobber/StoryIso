using System;
using StoryIso.Debugging;
using StoryIso.Misc;

namespace StoryIso.Scripting.Variables;

public struct ConstantVariable<T> : IVariable where T : notnull
{
	private readonly string _name;
	public string Name => _name;

	public Type ValueType => typeof(T);

	public bool IsReadOnly => false;

	public bool IsConstant => true;

	private readonly T _value;
	public IOptional Value => new Optional<T>(_value);

	public ConstantVariable(string name, T value) 
	{
		this._name = name;
		this._value = value;
	}

	public void Set(Source source, IOptional value)
	{
		DebugConsole.Raise(new ReadOnlyVariableError(source, this._name));
	}
}