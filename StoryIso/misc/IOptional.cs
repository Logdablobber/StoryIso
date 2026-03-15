using System;

namespace StoryIso.Misc;

public interface IOptional
{
	public bool HasValue { get; }
	public Type ValueType { get; }
}