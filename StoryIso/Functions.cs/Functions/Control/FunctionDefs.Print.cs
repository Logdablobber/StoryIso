using Microsoft.Xna.Framework;
using StoryIso.Debugging;
using StoryIso.Misc;

namespace StoryIso.Functions;

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
		function = (args, _) => 
		{
			Optional<string> arg = ParameterProcessor.Convert<string>(args![0]);

			if (!arg.HasValue)
			{
				return null;
			}

			DebugConsole.WriteLine(arg.Value.Trim('"'), Color.Black);

			return null;
		}
	};
}