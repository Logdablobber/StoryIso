using System;
using StoryIso.Enums;
using StoryIso.Misc;

namespace StoryIso.Scripting;

static partial class OperatorDefs
{
	/// <summary>
	/// (+) Addition Function
	/// <para>Takes in a and b as floats and returns a+b</para>
	/// </summary>
	/// 
	/// <returns>float</returns>
	private static readonly OperatorDef Add = new()
	{
		oper = "+",
		inlineFunc = false,
		isConstant = true,
		parameters = [typeof(float), typeof(float)],
		returnType = typeof(float),
		function = (args, _) =>
		{
			var item1 = args[0].ToOptional<float>();
			var item2 = args[1].ToOptional<float>();

			if (!item1.HasValue || !item2.HasValue)
			{
				return new Optional<float>();
			}

			return new Optional<float>(item1.Value + item2.Value);
		}
	};
}