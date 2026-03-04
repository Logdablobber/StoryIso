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
	/// StopMusic
	/// <para>Stops the currently playing bgm, if it exists.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef StopMusic = new()
	{
		name = "StopMusic",
		parameters = [],
		function = (_, _) => 
		{
			AudioManager.StopBGM();
			return null;	
		}
	};
}