using System.Text.Json;

namespace StoryIso.Dialogue;

public static class DialogueGenerator
{
	public static SerializableDialogueTree? Generate(string json)
	{
		return JsonSerializer.Deserialize<SerializableDialogueTree>(json, Game1.DeserializeOptions);
	}
}