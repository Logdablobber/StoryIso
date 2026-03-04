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
	/// PausesMusic
	/// <para>Pauses the currently playing bgm, if it exists.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef PauseMusic = new()
	{
		name = "PauseMusic",
		parameters = [],
		function = (_, _) => 
		{
			AudioManager.PauseBGM();
			return null;	
		}
	};
}