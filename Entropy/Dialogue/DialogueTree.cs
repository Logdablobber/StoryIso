using System;
using System.Collections.Generic;
using Entropy.Debugging;

namespace Entropy.Dialogue;

public class DialogueTree
{
	public readonly string id;

	readonly Dictionary<int, DialogueNode> Nodes = [];

	public DialogueNode currentNode;
	public bool AtEnd = false;

	public DialogueTree(string id, DialogueNode[] nodes)
	{
		foreach (var node in nodes)
		{
			if (Nodes.ContainsKey(node.ID))
			{
				throw new Exception("Duplicate nodes"); // TODO: switch these out for debug errors
			}

			Nodes.Add(node.ID, node);
		}

		if (!Nodes.TryGetValue(0, out var value))
		{
			throw new NullReferenceException("Start nodes does not exist");
		}

		currentNode = value;
		this.id = id;
	}

	public void Reset()
	{
		currentNode = Nodes[0];
		AtEnd = false;
	}

	public bool TryNext(Source source)
	{
		if (!currentNode.NextNode.HasValue && currentNode.Options != null)
		{
			return false;
		}

		if (!currentNode.NextNode.HasValue)
		{
			throw new NullReferenceException("Next node doesn't exist");
		}

		if (currentNode.NextNode.Value == -1)
		{
			AtEnd = true;
			return false;
		}

		if (!Nodes.TryGetValue(currentNode.NextNode.Value, out var next_node))
		{
			DebugConsole.Raise(new UnknownDialogueError(source, "Dialogue.Next", currentNode.NextNode.Value.ToString()));
			return false;
		}

		currentNode = next_node;
		return true;
	}

	public void Next(int option_index, Source source)
	{
		if (currentNode.Options == null || currentNode.Options.Length <= option_index)
		{
			DebugConsole.Raise(new UnknownDialogueError(source, "Dialogue.Next", option_index.ToString(), "Option doesn't exist"));
			return;
		}

		if (option_index == -1)
		{
			AtEnd = true;
		}

		if (!Nodes.TryGetValue(currentNode.Options[option_index].NextNode, out var next_node))
		{
			DebugConsole.Raise(new UnknownDialogueError(source, "Dialogue.Next", currentNode.Options[option_index].Text, "Next node doesn't exist"));
			return;
		}

		currentNode = next_node;
	}
}