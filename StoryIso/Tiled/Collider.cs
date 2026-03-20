using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace StoryIso.Tiled;

public class Collider
{
	public RectangleF hitbox;
	public BoundingRectangle BoundingBox
	{
		get
		{
			return new BoundingRectangle(hitbox.Center, hitbox.Size / 2);
		}
	}
	private bool _enabled;
	public bool Enabled => _enabled;

	public Collider(RectangleF hitbox, bool enabled)
	{
		this.hitbox = hitbox;
		_enabled = enabled;
	}

	public bool CheckCollision(Rectangle other)
	{
		if (!_enabled)
		{
			return false;
		}

		return hitbox.Intersects(other);
	}

	public void ToggleEnabled()
	{
		_enabled = !_enabled;
	}

	public void SetEnabled(bool value)
	{
		_enabled = value;
	}
}