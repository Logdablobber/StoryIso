using Microsoft.Xna.Framework;
using Entropy.Debugging;
using Entropy.Misc;

namespace Entropy.Scripting;

static partial class FunctionDefs
{
	/// <summary>
	/// Print
	/// <para>Takes in a string and prints it to the console</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef Print = new()
	{
		name = "Print",
		parameters = [typeof(string)],
		function = (_, args, source) => 
		{
			var arg = args[0].ToOptional<string>();

			if (!arg.HasValue)
			{
				return null;
			}

			DebugConsole.WriteLine(arg.Value, Color.Black);

			return null;
		}
	};
}