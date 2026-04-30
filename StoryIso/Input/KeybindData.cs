using System.Text.Json.Serialization;
using Microsoft.Xna.Framework.Input;

namespace StoryIso.Input;

public struct KeybindData
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public required Keys Key { get; set; }
    public string[]? KeyUp { get; set; }
    public string[]? KeyDown { get; set; }
    public string[]? KeyHeld { get; set; }
}

public struct KeybindsData
{
	public required KeybindData[] Keybinds { get; set; }
}