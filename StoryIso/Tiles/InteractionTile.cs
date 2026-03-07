using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using StoryIso.Enums;
using StoryIso.Functions;
using StoryIso.Misc;

namespace StoryIso.Tiled;

public class InteractionTile
{
	public uint id;
	Rectangle interactionHitbox;
	List<Function>? onInteract;
	List<Function>? onUninteract;
	List<Function>? whileInteract;
	List<Function>? onToggleOn;
	List<Function>? onToggleOff;
	bool interactingLastFrame = false;
	bool toggleState;

	const float INTERACTIONRANGE = 2f;
	const int LEVELOFDETAIL = 5;

	public InteractionTile(uint id,
							Rectangle rect, 
							List<Function>? on_interact = null, 
							List<Function>? on_uninteract = null, 
							List<Function>? while_interact = null,
							List<Function>? on_toggle_on = null,
							List<Function>? on_toggle_off = null,
							bool default_toggle_state = false)
	{
		this.id = id;
		interactionHitbox = rect;
		onInteract = on_interact;
		onUninteract = on_uninteract;
		whileInteract = while_interact;
		onToggleOn = on_toggle_on;
		onToggleOff = on_toggle_off;
		toggleState = default_toggle_state;
	}

	public void CheckInteract(Rectangle other, Direction facing_direction)
	{
		var keystate = Keyboard.GetState();

		if (!(keystate.IsKeyDown(Keys.Enter) || keystate.IsKeyDown(Keys.Z))) 
		{
			RunUninteract();
			return; // interaction keys not pressed
		}

		if (interactionHitbox.Intersects(other))
		{
			RunInteraction();
			return;
		}

		/*
		Check if the object is facing this object and within a certain distance
		via raycasting cuz it's cool B)
		*/

		List<Ray2> rays = [];

		switch (facing_direction) 
		{
			case Direction.Up:
				
				if (other.Bottom < interactionHitbox.Top)
				{
					RunUninteract();
					return;
				}

				for (int i = 0; i < LEVELOFDETAIL; i++)
				{
					rays.Add(new Ray2(new Vector2(MiscFuncs.Lerp(other.Left, other.Right, (float)i / (LEVELOFDETAIL - 1)), other.Top),
										new Vector2(0, -1)));
				}
				break;
			
			case Direction.Down:

				if (other.Top > interactionHitbox.Bottom)
				{
					RunUninteract();
					return;
				}

				for (int i = 0; i < LEVELOFDETAIL; i++)
				{
					rays.Add(new Ray2(new Vector2(MiscFuncs.Lerp(other.Left, other.Right, (float)i / (LEVELOFDETAIL - 1)), other.Bottom),
										new Vector2(0, 1)));
				}
				break;
			
			case Direction.Left:
				
				if (other.Right < interactionHitbox.Left)
				{
					RunUninteract();
					return;
				}

				for (int i = 0; i < LEVELOFDETAIL; i++)
				{
					rays.Add(new Ray2(new Vector2(other.Left, MiscFuncs.Lerp(other.Top, other.Bottom, (float)i / (LEVELOFDETAIL - 1))),
										new Vector2(-1, 0)));
				}
				break;
			
			case Direction.Right:

				if (other.Left > interactionHitbox.Right)
				{
					RunUninteract();
					return;
				}

				for (int i = 0; i < LEVELOFDETAIL; i++)
				{
					rays.Add(new Ray2(new Vector2(other.Right, MiscFuncs.Lerp(other.Top, other.Bottom, (float)i / (LEVELOFDETAIL - 1))),
										new Vector2(1, 0)));
				}
				break;
			
			default:
				RunUninteract();
				return;
		}

		var boundingRect = new BoundingRectangle(interactionHitbox.Center.ToVector2(), interactionHitbox.Size.ToVector2() / 2);

		foreach (var ray in rays)
		{
			if (ray.Intersects(boundingRect, out float dist, out float _))
			{
				if (dist <= INTERACTIONRANGE)
				{
					RunInteraction();
					return;
				}
			}
		}

		RunUninteract();
	}

	private void RunUninteract()
	{
		if (onUninteract == null)
		{
			return;
		}

		if (interactingLastFrame)
		{
			FunctionProcessor.RunFuncts(onUninteract, $"onUninteract of Interaction {id}");

			interactingLastFrame = false;
		}
	}

	private void RunInteraction()
	{
		if (onInteract == null || onToggleOff == null || onToggleOn == null || whileInteract == null)
		{
			return;
		}

		if (!interactingLastFrame)
		{
			FunctionProcessor.RunFuncts(onInteract, $"onInteract of Interaction {id}");

			FunctionProcessor.RunFuncts(toggleState ? onToggleOff : onToggleOn, $"onToggle{(toggleState ? "Off" : "On")} of Interaction {id}");

			toggleState = !toggleState;
			interactingLastFrame = true;
		}

		FunctionProcessor.RunFuncts(whileInteract, $"whileInteract of Interaction {id}", sync:true);
	}
}