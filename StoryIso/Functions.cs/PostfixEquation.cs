using System;

namespace StoryIso.Functions;

public class PostfixEquation<T>
{
	private (object, Type)[] equation;

	public PostfixEquation((object, Type)[] equation)
	{
		this.equation = equation;
	}

	public bool Solve(out T result)
	{
		// TODO: Do
		result = default;

		return true;
	}
}