using StoryIso.Misc;

namespace StoryIso.Scripting;

static partial class OperatorDefs
{
	/// <summary>
	/// (!) Not Function
	/// <para>Takes in a boolean value and returns !value</para>
	/// </summary>
	/// 
	/// <returns>bool</returns>
	private static readonly OperatorDef Not = new()
	{
		oper = "!",
		inlineFunc = false,
		isConstant = true,
		parameters = [typeof(bool)],
		returnType = typeof(bool),
		function = (args, _) =>
		{
			var item1 = args[0].ToOptional<bool>();

			if (!item1.HasValue)
			{
				return new Optional<bool>();
			}

			return new Optional<bool>(!item1.Value);
		}
	};
}