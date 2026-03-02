using System;
using StoryIso.Misc;

namespace StoryIso.Functions;

static partial class OperatorDefs
{
	/// <summary>
	/// (round) Round function
	/// <para>Takes in a value as a float and returns it rounded to the nearest integer. It is an inline function.</para>
	/// </summary>
	/// 
	/// <returns>float</returns>
	private static readonly OperatorDef Round = new()
	{
		oper = "round",
		inlineFunc = true,
		parameters = [typeof(float), typeof(int)],
		returnType = typeof(float),
		function = (args, _) =>
		{
			var item1 = (Optional<float>)args![0];
			var item2 = (Optional<int>)args[1];

			if (!item1.HasValue || !item2.HasValue)
			{
				return new Optional<float>();
			}

			return new Optional<float>(MathF.Round(item1.Value, item2.Value));
		}
	};
}