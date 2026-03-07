using StoryIso.Misc;

namespace StoryIso.Functions;

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
		parameters = [typeof(bool)],
		returnType = typeof(bool),
		function = (args, _) =>
		{
			var item1 = (Optional<bool>)args![0];

			if (!item1.HasValue)
			{
				return new Optional<bool>();
			}

			return new Optional<bool>(!item1.Value);
		}
	};
}