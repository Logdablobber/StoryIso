using System;
using StoryIso.Misc;

namespace StoryIso.Functions;

static partial class OperatorDefs
{
	/// <summary>
	/// (floor) Floor function
	/// <para>Takes in a value as a float and returns it rounded down to the nearest integer. It is an inline function.</para>
	/// </summary>
	/// 
	/// <returns>float</returns>
	private static readonly OperatorDef Floor = new()
	{
		oper = "floor",
		inlineFunc = true,
		parameters = [typeof(float)],
		returnType = typeof(float),
		function = (args, _) =>
		{
			var item1 = (Optional<float>)args![0];

			if (!item1.HasValue)
			{
				return new Optional<float>();
			}

			return new Optional<float>(MathF.Floor(item1.Value));
		}
	};
}