using StoryIso.Misc;

namespace StoryIso.Functions;

static partial class OperatorDefs
{
	/// <summary>
	/// (==) Equal
	/// <para>Takes in a and b and returns a == b</para>
	/// </summary>
	/// 
	/// <returns>bool</returns>
	private static readonly OperatorDef Equal = new()
	{
		oper = "==",
		inlineFunc = false,
		parameters = [typeof(VariableObject), typeof(VariableObject)],
		returnType = typeof(bool),
		function = (args, _) =>
		{
			ParameterProcessor.ConvertUnknown(args![0], out var item1);
			ParameterProcessor.ConvertUnknown(args[1], out var item2);

			if (item1 == null || item2 == null)
			{
				return new Optional<bool>();
			}

			return new Optional<bool>(item1 == item2);
		}
	};
}