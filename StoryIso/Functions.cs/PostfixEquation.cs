using System;
using StoryIso.Debugging;
using StoryIso.Misc;

namespace StoryIso.Functions;

public class PostfixEquation<T> where T : notnull
{
	private readonly (object, Type)[] equation;

	public PostfixEquation((object, Type)[] equation)
	{
		this.equation = equation;
	}

	public bool Evaluate(Source source, out Optional<T> result)
	{
		return ParameterEvaluator.Evaluate(source, equation, out result);
	}
}