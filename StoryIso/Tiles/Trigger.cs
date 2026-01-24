using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using StoryIso.Debugging;
using StoryIso.ECS;
using StoryIso.Enums;
using StoryIso.Functions;

namespace StoryIso.Tiled;

public class Trigger
{
	public int id;
	const float TRIGGERALPHA = 0.40f;
	private Color _debugColor;

	private Rectangle triggerHitbox;

	private bool previous_state = false;

	private readonly List<Function> _onEnter;
	private readonly List<Function> _onExit;
	private readonly List<Function> _onStay;

	public Trigger(int id, Rectangle hitbox, List<Function> onEnter, List<Function> onExit, List<Function> onStay, Color color)
	{
		this.id = id;
		triggerHitbox = hitbox;
		_onEnter = onEnter;
		_onExit = onExit;
		_onStay = onStay;
		_debugColor = color * TRIGGERALPHA;
	}

	public void CheckCollision(GameTime gameTime, Rectangle other)
	{
		if (triggerHitbox.Intersects(other))
		{
			RunOnStay();

			if (!previous_state)
			{
				RunOnEnter();
			}

			previous_state = true;
			return;
		}

		if (previous_state)
		{
			RunOnExit();
			previous_state = false;
		}
	}

	private void RunOnEnter()
	{
		FunctionProcessor.RunFuncts(_onEnter, $"onEnter of Trigger {id}");
	}

	private void RunOnExit()
	{
		FunctionProcessor.RunFuncts(_onExit, $"onExit of Trigger {id}");
	}

	private void RunOnStay()
	{
		FunctionProcessor.RunFuncts(_onStay, $"onStay of Trigger {id}", sync:true);
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.FillRectangle(triggerHitbox, _debugColor, 0);
	}
}