using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StoryIso.Enums;

namespace StoryIso.Entities;

public class Player
{
	public float Speed;

	public Player(float speed)
	{
		Speed = speed;
	}
}