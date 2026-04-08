using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Serialization.Json;
using StoryIso.Enums;
using StoryIso.Misc;

namespace StoryIso.UI;

public struct UIData
{
	public required string Name { get; set; }
	public float? Scale { get; set; }
	public bool? Visible { get; set; }
	public required UIPart[] Parts { get; set; }
	public required float[] Position { get; set; }
}

public struct UIPart
{
	public required string Name { get; set; }
	public required IContent Content { get; set; }
	public required float[] Position { get; set; }
	public float? Scale { get; set; } // default = 1
	public bool? Visible { get; set; } // default = true;
}

[JsonDerivedType(typeof(ImageContent), typeDiscriminator: "image")]
[JsonDerivedType(typeof(TextContent), typeDiscriminator: "text")]
[JsonDerivedType(typeof(ButtonContent), typeDiscriminator: "button")]
[JsonDerivedType(typeof(RectangleContent), typeDiscriminator: "rectangle")]
[JsonDerivedType(typeof(PolygonContent), typeDiscriminator: "polygon")]
public interface IContent
{
	
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
	public required float fontSize { get; set; }
	public required float[] Size { get; set; }

	[JsonConverter(typeof(ColorJsonConverter))]
	public Color? color { get; set; }
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public VerticalTextAlignment? vAlignment { get; set; }
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public HorizontalTextAlignment? hAlignment { get; set; }
}

public struct ButtonContent : IContent
{
	public required float[] Size { get; set; }
	public string? OnLeftClick { get; set; }
	public string? OnLeftRelease { get; set; }
	public string? WhileLeftHeld { get; set; }
	public string? OnRightClick { get; set; }
	public string? OnRightRelease { get; set; }
	public string? WhileRightHeld { get; set; }
	public string? OnEnter { get; set; }
	public string? OnExit { get; set; }
	public string? OnStay { get; set; }
}

public struct RectangleContent : IContent
{
	[JsonConverter(typeof(ColorJsonConverter))]
	public required Color color { get; set; }
	public required float[] Size { get; set; }

	public uint? OutlineWidth { get; set; }
	public Color? OutlineColor { get; set; }
}

public struct PolygonContent : IContent
{
	[JsonConverter(typeof(ColorJsonConverter))]
	public required Color color { get; set; }
	public required float[][] Vertices { get; set; }
}