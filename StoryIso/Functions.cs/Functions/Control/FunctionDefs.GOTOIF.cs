using StoryIso.Misc;

namespace StoryIso.Functions;

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
			var item1 = (PostfixEquation<bool>)args![0];
			Optional<uint> item2 = ParameterProcessor.Convert<uint>(args[1]);

			if (item1 == null || !item2.HasValue)
			{
				return null;
			}

			if (!item1.Evaluate(source!, out Optional<bool> result) || !result.HasValue || result.Value)
			{
				return null;
			}

			return item2.Value;
		}
	};
}