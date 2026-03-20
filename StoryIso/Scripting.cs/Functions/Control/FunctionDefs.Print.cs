using Microsoft.Xna.Framework;
using StoryIso.Debugging;
using StoryIso.Misc;

namespace StoryIso.Scripting;

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
			Optional<string> arg = ParameterProcessor.Convert<string>(source, args![0]);

			if (!arg.HasValue)
			{
				return null;
			}

			DebugConsole.WriteLine(arg.Value, Color.Black);

			return null;
		}
	};
}