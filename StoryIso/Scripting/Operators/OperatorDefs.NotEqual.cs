using StoryIso.Misc;

namespace StoryIso.Scripting;

static partial class OperatorDefs
{
	/// <summary>
	/// (!=) Not Equal
	/// <para>Takes in a and b and returns a != b</para>
	/// </summary>
	/// 
	/// <returns>bool</returns>
	private static readonly OperatorDef NotEqual = new()
	{
		oper = "!=",
		inlineFunc = false,
		isConstant = true,
		parameters = [typeof(VariableObject), typeof(VariableObject)],
		returnType = typeof(bool),
		function = (args, source) =>
		{
			ParameterProcessor.ConvertUnknown(source, args[0], out var item1);
			ParameterProcessor.ConvertUnknown(source, args[1], out var item2);

			if (item1 == null || item2 == null)
			{
				return new Optional<bool>();
			}

			return new Optional<bool>(item1 != item2);
		}
	};
}