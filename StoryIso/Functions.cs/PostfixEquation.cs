using System;
using StoryIso.Debugging;

namespace StoryIso.Functions;

public class PostfixEquation<T>
{
	private (object, Type)[] equation;

	public PostfixEquation((object, Type)[] equation)
	{
		this.equation = equation;
	}

	public bool Evaluate(Source source, out T result)
	{
		return ParameterEvaluator.Evaluate(source, equation, out result);
	}
}