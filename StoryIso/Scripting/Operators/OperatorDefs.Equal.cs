using StoryIso.Misc;

namespace StoryIso.Scripting;

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
		isConstant = true,
		parameters = [typeof(VariableObject), typeof(VariableObject)],
		returnType = typeof(bool),
		function = (args, _) =>
		{
			var item1 = ParameterProcessor.ConvertByTypeToString(args[0]);
			var item2 = ParameterProcessor.ConvertByTypeToString(args[1]);

			if (item1 == null || item2 == null)
			{
				return new Optional<bool>();
			}

			return new Optional<bool>(item1 == item2);
		}
	};
}