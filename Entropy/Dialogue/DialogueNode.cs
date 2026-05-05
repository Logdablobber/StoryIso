
using Microsoft.Xna.Framework;

namespace Entropy.Dialogue;

public class DialogueNode
{
	public required int ID { get; set; }

	public string? speaker { get; set; }
	public required string text { get; set;}
	public float? SpeedMultiplier { get; set;}
	public Color? color { get; set;}
	public bool PreventSkip { get; set;}
	public float? Duration { get; set;}
	public DialogueOption[]? Options { get; set; }
	public int? NextNode { get; set; }

	// I may add extra things like backgrounds
	// or other things that could change per line
}