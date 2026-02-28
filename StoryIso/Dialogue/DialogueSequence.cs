using System.Collections.Generic;

namespace StoryIso.Dialogue;

public class DialogueSequence
{
	public required string id { get; set; }
	public required List<DialogueStep> dialogueSteps { get; set; }
}