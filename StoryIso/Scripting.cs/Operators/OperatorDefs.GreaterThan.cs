using StoryIso.Misc;

namespace StoryIso.Scripting;

static partial class OperatorDefs
{
	/// <summary>
	/// (>) Greater Than
	/// <para>Takes in a and b as floats and returns a > b</para>
	/// </summary>
	/// 
	/// <returns>bool</returns>
	private static readonly OperatorDef GreaterThan = new()
	{
		oper = ">",
		inlineFunc = false,
		isConstant = true,
		parameters = [typeof(float), typeof(float)],
		returnType = typeof(bool),
		function = (args, _) =>
		{
			var item1 = (Optional<float>)args![0];
			var item2 = (Optional<float>)args[1];

			if (!item1.HasValue || !item2.HasValue)
			{
				return new Optional<bool>();
			}

			return new Optional<bool>(item1.Value > item2.Value);
		}
	};
}