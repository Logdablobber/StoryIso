using Entropy.Audio;

namespace Entropy.Scripting;

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
		function = (_, _, _) => 
		{
			AudioManager.PauseBGM();
			return null;	
		}
	};
}