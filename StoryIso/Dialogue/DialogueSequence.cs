using System.Collections.Generic;

namespace StoryIso.Dialogue;

public class DialogueSequence
{
	public required string id;
	public required List<DialogueStep> dialogueSteps;
}