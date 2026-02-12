using Newtonsoft.Json;

namespace StoryIso.Dialogue;

public static class DialogueGenerator
{
	public static DialogueSequence? Generate(string json)
	{
		return JsonConvert.DeserializeObject<DialogueSequence>(json);
	}
}