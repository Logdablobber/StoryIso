using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace StoryIso.Entities;

public class RectangleComponent
{
	public SizeF size;
	public float? borderSize;
	public Color? borderColor;

	public RectangleComponent(SizeF size, float? border_size, Color? border_color)
	{
		this.size = size;
		this.borderSize = border_size;
		this.borderColor = border_color;
	}

	public void Draw(SpriteBatch spriteBatch, Color color, Vector2 position, Vector2 scale, float layer_depth)
	{
		if (!borderSize.HasValue || !borderColor.HasValue)
		{
			RectangleF rect = new(position, size * scale);

			spriteBatch.FillRectangle(rect, color, layer_depth);
			return;
		}

		RectangleF border_rectangle = new(position, size * scale);
		RectangleF inner_rectangle = new(position + new Vector2(borderSize.Value) * scale, (size - new Vector2(borderSize.Value * 2).ToSize()) * scale);

		spriteBatch.FillRectangle(border_rectangle, borderColor.Value, layer_depth);
		spriteBatch.FillRectangle(inner_rectangle, color, layer_depth);
	}
}