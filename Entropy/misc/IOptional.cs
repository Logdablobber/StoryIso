using System;

namespace Entropy.Misc;

public interface IOptional
{
	public bool HasValue { get; }
	public Type ValueType { get; }
}