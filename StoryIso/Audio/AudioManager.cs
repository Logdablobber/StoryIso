using Microsoft.Xna.Framework.Audio;
using StoryIso.Debugging;
using StoryIso.FileLoading;

namespace StoryIso.Audio;

public static class AudioManager
{
	public static string? BGMName { get; private set; }
	private static SoundEffectInstance? _backgroundMusic;

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

	public static void SetBGM(Source source, string name, float volume)
	{
		var sound = AudioLoader.GetSound(name);

		if (sound == null)
		{
			DebugConsole.Raise(new MissingAssetError(source, name, "Unknown sound"));
			return;
		}

		sound.Volume = volume;
		sound.IsLooped = true;

		BGMName = name;
		_backgroundMusic = sound;
	}

	public static void StopBGM()
	{
		_backgroundMusic?.Stop();
	}

	public static void PauseBGM()
	{
		_backgroundMusic?.Pause();
	}

	public static void PlayBGM()
	{
		_backgroundMusic?.Play();
	}

	public static void SetVolume(float volume)
	{
		SoundEffect.MasterVolume = volume;
	}

	public static float GetVolume()
	{
		return SoundEffect.MasterVolume;
	}
}