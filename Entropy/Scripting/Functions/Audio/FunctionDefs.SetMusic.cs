using Entropy.Audio;
using Entropy.Misc;

namespace Entropy.Scripting;

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
		function = (_, args, source) => 
		{
			var name = args[0].ToOptional<string>();
			var volume = args[1].ToOptional<float>();

			if (!name.HasValue || !volume.HasValue)
			{
				return null;
			}

			AudioManager.SetBGM(source!, name.Value, volume.Value);
			AudioManager.PlayBGM();
			return null;	
		}
	};
}