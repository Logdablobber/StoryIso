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
	/// PlaysMusic
	/// <para>Plays/resumes the currently set bgm, if it exists and isn't already playing.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef PlayMusic = new()
	{
		name = "PlayMusic",
		parameters = [],
		function = (_, _) => 
		{
			AudioManager.PlayBGM();
			return null;	
		}
	};
}