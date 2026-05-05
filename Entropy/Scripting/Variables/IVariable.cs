using System;
using Entropy.Debugging;
using Entropy.Misc;

namespace Entropy.Scripting.Variables;

public interface IVariable
{
	public string Name { get; }
	public Type ValueType { get; }
	public bool IsReadOnly { get; }
	public bool IsConstant { get; }
	public IOptional Value { get; }

	public abstract void Set(Source source, IOptional value);
}