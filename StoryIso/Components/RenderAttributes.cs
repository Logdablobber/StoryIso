using Microsoft.Xna.Framework;

namespace StoryIso.Entities;

public class RenderAttributes
{
	public bool Visible { get; set; }
	public Color color { get; set; }
	
	// this is whether the element is drawn to the screen (i.e. UI element)
	// or whether its drawn to the world (i.e. Character)
	public bool ScreenElement { get; set; }

	public RenderAttributes(bool visible, Color color)
	{
		this.Visible = visible;
		this.color = color;
		this.ScreenElement = false;
	}

	public RenderAttributes(bool visible, Color color, bool screen_element)
	{
		this.Visible = visible;
		this.color = color;
		this.ScreenElement = screen_element;
	}
}