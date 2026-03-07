using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Audio;

namespace StoryIso.FileLoading;

public static class AudioLoader
{
	private static readonly ConcurrentDictionary<string, SoundEffect> _sounds = [];

	public static SoundEffectInstance? GetSound(string name)
	{
		if (_sounds.TryGetValue(name, out var value))
		{
			return value.CreateInstance();
		}

		return null;
	}

	public static void LoadSounds(string path)
	{
		_sounds.Clear();

		var dirNames = new Regex("[.]wav", RegexOptions.Compiled | RegexOptions.IgnoreCase);
		string[] files = Directory.GetFiles(path).Where(dir => dirNames.IsMatch(dir)).ToArray();

		Task[] tasks = new Task[files.Length];

		for (int i = 0; i < files.Length; i++)
		{
			string f = files[i];

			var task = new Task(() =>
			{
				SoundEffect sound_file = SoundEffect.FromFile(f);

				FileInfo file = new(f);

				string sound_name = dirNames.Replace(file.Name, replacement:string.Empty);

				_sounds.TryAdd(sound_name, sound_file);
			});
			task.Start();

			tasks[i] = task;
		}

		Task.WaitAll(tasks);
	}
}