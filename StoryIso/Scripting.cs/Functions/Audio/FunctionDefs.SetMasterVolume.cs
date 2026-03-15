using StoryIso.Audio;
using StoryIso.Debugging;

namespace StoryIso.Scripting;

static partial class FunctionDefs
{
	/// <summary>
	/// SetMasterVolume
	/// <para>Takes in a float volume where volume >= 0 and sets the master volume to that value.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef SetMasterVolume = new()
	{
		name = "SetMasterVolume",
		parameters = [typeof(float)],
		function = (args, source) => 
		{
			var item1 = ParameterProcessor.Convert<float>(args![0]);

			if (!item1.HasValue)
			{
				return null;
			}

			if (item1.Value < 0)
			{
				DebugConsole.Raise(new ParameterValueError(source!, "SetMasterVolume", item1.Value.ToString(), "Master Volume must be greater than or equal to 0"));
			}

			AudioManager.SetVolume(item1.Value);
			return null;	
		}
	};
}