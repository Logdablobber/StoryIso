using System;
using StoryIso.Debugging;
using StoryIso.FileLoading;

namespace StoryIso.Audio;

public static class AudioManager
{
	// volume should be from 0 to 1
	// pitch should be from -10 to 10
	public static void PlaySound(Source source, string name, float volume, float pitch)
	{
		var sound = AudioLoader.GetSound(name);

		if (sound == null)
		{
			DebugConsole.Raise(new MissingAssetError(source, name, "Unknown sound"));
			return;
		}

		sound.Volume = volume;
		sound.IsLooped = false;
		sound.Pitch = pitch;

		sound.Play();
	}
}