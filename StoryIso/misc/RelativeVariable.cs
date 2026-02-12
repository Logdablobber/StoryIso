using StoryIso.Functions;

namespace StoryIso.Misc;


public struct RelativeVariable<T>
{
	public readonly bool Relative;

	public T Value { get; set; }

	public RelativeVariable(T value, bool relative)
	{
		Relative = relative;
		Value = value;
	}
}