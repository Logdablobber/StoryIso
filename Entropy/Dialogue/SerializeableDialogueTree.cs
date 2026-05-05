using System.Collections.Generic;

namespace Entropy.Dialogue;

public class SerializableDialogueTree
{
	public required string id { get; set; }
	public required DialogueNode[] nodes { get; set; }

	public DialogueTree ToDialogueTree()
	{
		return new DialogueTree(id, nodes);
	}
}