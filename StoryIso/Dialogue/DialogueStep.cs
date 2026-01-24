using System.Collections.Generic;
using System.Data;

namespace StoryIso.Dialogue;

public struct DialogueStep
{
	public string speaker;
	public string text;
	public float? speedMultiplier;
	public DialogueColor? color;
	public bool preventSkip;
	public float? duration;
	// I may add extra things like backgrounds
	// or other things that could change per line
}

