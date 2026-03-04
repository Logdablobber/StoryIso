using Microsoft.Xna.Framework;
using StoryIso.Debugging;
using StoryIso.ECS;
using StoryIso.Enums;
using StoryIso.Misc;
using StoryIso.Scenes;

namespace StoryIso.Functions;

static partial class FunctionDefs
{
	/// <summary>
	/// GOTO
	/// <para> Goes to a given line.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef GOTO = new()
	{
		name = "GOTO",
		parameters = [typeof(uint)],
		function = (args, source) => 
		{
			Optional<uint> line = ParameterProcessor.Convert<uint>(args![0]);

			if (!line.HasValue)
			{
				return null;
			}

			return line.Value;
		}
	};
}