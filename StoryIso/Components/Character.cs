using Microsoft.Xna.Framework;
using StoryIso.Enums;
using StoryIso.Scenes;

namespace StoryIso.Entities;

public class Character
{
	public string Name;
	public Direction Direction;
	public AbsoluteMovement? Movement;
	private bool moving = false;
	public bool Moving
	{
		get
		{
			return moving || Movement.HasValue;
		}
		set
		{
			moving = value;
		}
	}
	public bool Visible;

	public Character(string name, Direction start_direction, bool visibility)
	{
		Name = name;
		Direction = start_direction;
		Movement = null;
		Visible = visibility;
	}
}