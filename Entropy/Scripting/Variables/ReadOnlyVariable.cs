using System;
using Entropy.Debugging;
using Entropy.Misc;

namespace Entropy.Scripting.Variables;

public struct ReadOnlyVariable<T> : IVariable where T : notnull
{
	private readonly string _name;
	public string Name => _name;

	public Type ValueType => typeof(T);

	public bool IsReadOnly => true;

	public bool IsConstant => false;

	private readonly Func<Optional<T>> _valueFunc;
	public IOptional Value => _valueFunc();

	public ReadOnlyVariable(string name, Func<Optional<T>> value) 
	{
		this._name = name;
		this._valueFunc = value;
	}

	public void Set(Source source, IOptional value)
	{
		DebugConsole.Raise(new ReadOnlyVariableError(source, this._name));
	}
}