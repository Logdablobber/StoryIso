using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Serialization.Json;

namespace StoryIso.UI;

public struct UIData
{
	public required string Name { get; set; }
	public float? Scale { get; set; }
	public bool? Visible { get; set; }
	public required UIPart[] Parts { get; set; }
	public required Vector2 Position { get; set; }
}

public struct UIPart
{
	public required string Name { get; set; }
	public required IContent Content { get; set; }

	[JsonConverter(typeof(Vector2JsonConverter))]
	public required Vector2 Position { get; set; }
	public float? Scale { get; set; } // default = 1
	public bool? Visible { get; set; } // default = true;
}

[JsonDerivedType(typeof(ImageContent), typeDiscriminator: "image")]
[JsonDerivedType(typeof(TextContent), typeDiscriminator: "text")]
public interface IContent
{
	[JsonConverter(typeof(ColorJsonConverter))]
	public Color? color { get; set; }
}

public struct ImageContent : IContent
{
	public required string image { get; set; }

	[JsonConverter(typeof(ColorJsonConverter))]
	public Color? color { get; set; }
}

public struct TextContent : IContent
{
	public required string text { get; set; }
	public required string font { get; set; }

	[JsonConverter(typeof(Vector2JsonConverter))]
	public required Vector2 Size { get; set; }

	[JsonConverter(typeof(ColorJsonConverter))]
	public Color? color { get; set; }
}