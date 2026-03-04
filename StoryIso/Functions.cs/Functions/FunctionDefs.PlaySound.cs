using Microsoft.Xna.Framework;
using StoryIso.Audio;
using StoryIso.Debugging;
using StoryIso.ECS;
using StoryIso.Enums;
using StoryIso.Misc;
using StoryIso.Scenes;

namespace StoryIso.Functions;

static partial class FunctionDefs
{
	/// <summary>
	/// PlaySound
	/// <para>Takes in the sound name as a string, and the volume and pitch as floats. The sound is played once, the volume should be between 0 and 1, and pitch should be between -10 and 10.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef PlaySound = new()
	{
		name = "PlaySound",
		parameters = [typeof(string), typeof(float), typeof(float)],
		function = (args, source) => 
		{
			var item1 = ParameterProcessor.Convert<string>(args![0]);
			var item2 = ParameterProcessor.Convert<float>(args[1]);
			var item3 = ParameterProcessor.Convert<float>(args[2]);

			if (!item1.HasValue || !item2.HasValue || !item3.HasValue)
			{
				return null;
			}

			AudioManager.PlaySound(source!, item1.Value, item2.Value, item3.Value);
			return null;	
		}
	};
}