using System;
using MonoGame.Extended;
using StoryIso.Misc;

namespace StoryIso.Scripting;

static partial class OperatorDefs
{
	/// <summary>
	/// (randint) Random Integer function
	/// <para>Takes 2 integers and returns a random integer in that range. It is an inline function.</para>
	/// </summary>
	/// 
	/// <returns>int</returns>
	private static readonly OperatorDef RandInt = new()
	{
		oper = "randint",
		inlineFunc = true,
		isConstant = false,
		parameters = [typeof(int), typeof(int)],
		returnType = typeof(int),
		function = (args, _) =>
		{
			var item1 = args[0].ToOptional<int>();
			var item2 = (Optional<int>)args[1];

			if (!item1.HasValue || !item2.HasValue)
			{
				return new Optional<int>();
			}

			return new Optional<int>(Random.Shared.Next(item1.Value, item2.Value));
		}
	};
}