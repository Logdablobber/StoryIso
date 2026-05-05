using Entropy.Misc;

namespace Entropy.Scripting;

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
		parameters = [typeof(bool), typeof(uint)],
		function = (_, args, source) =>
		{
			var condition = args[0].ToOptional<bool>();
			var line = args[1].ToOptional<uint>();

			if (!condition.HasValue || !line.HasValue || !condition.Value)
			{
				return null;
			}

			return line.Value;
		}
	};
}