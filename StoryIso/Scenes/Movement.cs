using Microsoft.Xna.Framework;
using StoryIso.Misc;

namespace StoryIso.Scenes;

public struct Movement
{
	public RelativeVector2 movement;
	public float speed;

	public Movement(RelativeVector2 movement, float speed)
	{
		this.movement = movement;
		this.speed = speed;
	}

	public Movement(Vector2 movement, float speed)
	{
		this.movement = new RelativeVector2(movement);
		this.speed = speed;
	}

	public AbsoluteMovement ToAbsolute(Vector2 origin)
	{
		return new AbsoluteMovement(movement.ToAbsolute(origin), speed);
	}
}

public struct AbsoluteMovement
{
	public Vector2 movement;
	public float speed;

	public AbsoluteMovement(Vector2 movement, float speed)
	{
		this.movement = movement;
		this.speed = speed;
	}
}