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
	/// SetMusic
	/// <para>Takes in the audio name as a string and volume as a float from 0 to 1 and plays it, replacing the current bgm if it exists.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef SetMusic = new()
	{
		name = "SetMusic",
		parameters = [typeof(string), typeof(float)],
		function = (args, source) => 
		{
			var item1 = ParameterProcessor.Convert<string>(args![0]);
			var item2 = ParameterProcessor.Convert<float>(args[1]);

			if (!item1.HasValue || !item2.HasValue)
			{
				return null;
			}

			AudioManager.SetBGM(source!, item1.Value, item2.Value);
			AudioManager.PlayBGM();
			return null;	
		}
	};
}