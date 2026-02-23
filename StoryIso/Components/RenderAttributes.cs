using Microsoft.Xna.Framework;

namespace StoryIso.Entities;

public class RenderAttributes
{
	public bool Visible { get; set; }
	public Color color { get; set; }

	public RenderAttributes(bool visible, Color color)
	{
		this.Visible = visible;
		this.color = color;
	}
}