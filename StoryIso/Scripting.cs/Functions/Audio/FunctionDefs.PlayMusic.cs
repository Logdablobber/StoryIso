using StoryIso.Audio;

namespace StoryIso.Scripting;

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