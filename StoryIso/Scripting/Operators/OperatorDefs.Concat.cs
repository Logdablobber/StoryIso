using System;
using StoryIso.Misc;

namespace StoryIso.Scripting;

static partial class OperatorDefs
{
	/// <summary>
	/// (concat) String Concatenation function
	/// <para>Takes in a 2 strings and returns the concatenation of them. It is an inline function.</para>
	/// </summary>
	/// 
	/// <returns>string</returns>
	private static readonly OperatorDef Concat = new()
	{
		oper = "concat",
		inlineFunc = true,
		isConstant = true,
		parameters = [typeof(string), typeof(string)],
		returnType = typeof(string),
		function = (args, _) =>
		{
			var item1 = args[0].ToOptional<string>();
			var item2 = (Optional<string>)args[1];

			if (!item1.HasValue || !item2.HasValue)
			{
				return new Optional<string>();
			}

			return new Optional<string>(item1.Value + item2.Value);
		}
	};
}