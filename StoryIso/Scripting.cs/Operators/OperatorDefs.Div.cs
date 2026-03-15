using StoryIso.Misc;

namespace StoryIso.Scripting;

static partial class OperatorDefs
{
	/// <summary>
	/// (/) Division Function
	/// <para>Takes in a and b as floats and returns a/b</para>
	/// </summary>
	/// 
	/// <returns>float</returns>
	private static readonly OperatorDef Div = new()
	{
		oper = "/",
		inlineFunc = false,
		isConstant = true,
		parameters = [typeof(float), typeof(float)],
		returnType = typeof(float),
		function = (args, _) =>
		{
			var item1 = (Optional<float>)args![0];
			var item2 = (Optional<float>)args[1];

			if (!item1.HasValue || !item2.HasValue)
			{
				return new Optional<float>();
			}

			return new Optional<float>(item1.Value / item2.Value);
		}
	};
}