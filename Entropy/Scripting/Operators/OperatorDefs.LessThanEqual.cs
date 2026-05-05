using Entropy.Misc;

namespace Entropy.Scripting;

static partial class OperatorDefs
{
	/// <summary>
	/// (<=) Less Than Or Equal To
	/// <para>Takes in a and b as floats and returns a <= b</para>
	/// </summary>
	/// 
	/// <returns>bool</returns>
	private static readonly OperatorDef LessThanEqual = new()
	{
		oper = "<=",
		inlineFunc = false,
		isConstant = true,
		sync = true,
		parameters = [typeof(float), typeof(float)],
		returnType = typeof(bool),
		function = (args, _) =>
		{
			var item1 = args[0].ToOptional<float>();
			var item2 = args[1].ToOptional<float>();

			if (!item1.HasValue || !item2.HasValue)
			{
				return new Optional<bool>();
			}

			return new Optional<bool>(item1.Value <= item2.Value);
		}
	};
}