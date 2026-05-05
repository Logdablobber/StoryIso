using Entropy.Audio;
using Entropy.Misc;

namespace Entropy.Scripting;

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
		function = (_, args, source) => 
		{
			var name = args[0].ToOptional<string>();
			var volume = args[1].ToOptional<float>();
			var pitch = args[2].ToOptional<float>();

			if (!name.HasValue || !volume.HasValue || !pitch.HasValue)
			{
				return null;
			}

			AudioManager.PlaySound(source!, name.Value, volume.Value, pitch.Value);
			return null;	
		}
	};
}