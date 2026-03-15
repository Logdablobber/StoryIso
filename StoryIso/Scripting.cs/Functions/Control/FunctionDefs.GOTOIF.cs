using StoryIso.Misc;

namespace StoryIso.Scripting;

static partial class FunctionDefs
{
	/// <summary>
	/// GOTOIF
	/// <para> Goes to a given line if a condition is fulfilled. Not a function that can be run</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef GOTOIF = new()
	{
		name = "GOTOIF",
		parameters = [typeof(object), typeof(uint)],
		function = (args, source) =>
		{
			Optional<bool> item1 = ParameterProcessor.Convert<bool>(args![0]);
			Optional<uint> item2 = ParameterProcessor.Convert<uint>(args[1]);

			if (!item1.HasValue || !item2.HasValue || !item1.Value)
			{
				return null;
			}

			return item2.Value;
		}
	};
}