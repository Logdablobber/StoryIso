using Entropy.Enums;
using Entropy.Scenes;

namespace Entropy.Entities;

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
	public string Room;

	public bool? Visible = null;

	public Character(string name, Direction start_direction, string room)
	{
		Name = name;
		Direction = start_direction;
		Movement = null;
		Room = room;
	}
}