using Microsoft.Xna.Framework;
using StoryIso.Enums;

namespace StoryIso.Entities;

public class RenderAttributes
{
	public bool Visible { get; set; }
	public Color color { get; set; }
	
	// this is whether the element is drawn to the screen (i.e. UI element)
	// or whether its drawn to the world (i.e. Character)
	public bool ScreenElement { get; set; }
	private Vector2? _origin { get; set; }
	public RenderLayer renderLayer { get; set; }

	public RenderAttributes(bool visible, Color color, RenderLayer layer, Vector2? origin = null)
	{
		this.Visible = visible;
		this.color = color;
		this.ScreenElement = false;
		this._origin = origin;
		this.renderLayer = layer;
	}

	public RenderAttributes(bool visible, Color color, bool screen_element, RenderLayer layer, Vector2? origin = null)
	{
		this.Visible = visible;
		this.color = color;
		this.ScreenElement = screen_element;
		this._origin = origin;
		this.renderLayer = layer;
	}

	public Vector2 GetOrigin(Vector2 size)
	{
		if (!this._origin.HasValue)
		{
			return Vector2.Zero;
		}

		return size * this._origin.Value;
	}
}