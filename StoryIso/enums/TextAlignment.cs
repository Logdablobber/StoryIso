namespace StoryIso.Enums;

public struct TextAlignment
{
	public HorizontalTextAlignment HorizontalAlignment;
	public VerticalTextAlignment VerticalAlignment;
}

public enum HorizontalTextAlignment
{
	None = 0,
	Left = 1,
	Center = 2,
	Right = 3,
}

public enum VerticalTextAlignment
{
	None = 0,
	Top = 1,
	Center = 2,
	Bottom = 3,
}