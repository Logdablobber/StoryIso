using StoryIso.Misc;

namespace StoryIso.Scripting;

static partial class OperatorDefs
{
	/// <summary>
	/// (&&) And
	/// <para>Takes in a and b as bools and returns a && b</para>
	/// </summary>
	/// 
	/// <returns>bool</returns>
	private static readonly OperatorDef And = new()
	{
		oper = "&&",
		inlineFunc = false,
		isConstant = true,
		parameters = [typeof(bool), typeof(bool)],
		returnType = typeof(bool),
		function = (args, _) =>
		{
			var item1 = (Optional<bool>)args![0];
			var item2 = (Optional<bool>)args[1];

			if (!item1.HasValue || !item2.HasValue)
			{
				return new Optional<bool>();
			}

			return new Optional<bool>(item1.Value && item2.Value);
		}
	};
}