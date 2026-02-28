using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Serialization.Json;

namespace StoryIso.Dialogue;

public struct DialogueStep
{
	public required string speaker { get; set; }
	public required string text { get; set;}
	public float? speedMultiplier { get; set;}

	[JsonConverter(typeof(ColorJsonConverter))]
	public Color? color { get; set;}
	public bool preventSkip { get; set;}
	public float? duration { get; set;}
	// I may add extra things like backgrounds
	// or other things that could change per line
}

