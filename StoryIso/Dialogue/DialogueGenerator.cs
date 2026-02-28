using System.Text.Json;

namespace StoryIso.Dialogue;

public static class DialogueGenerator
{
	public static DialogueSequence? Generate(string json)
	{
		return JsonSerializer.Deserialize<DialogueSequence>(json, Game1.DeserializeOptions);
	}
}