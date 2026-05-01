using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using StoryIso.Debugging;
using StoryIso.Scripting;

namespace StoryIso.Tiled;

public class Trigger
{
	public uint id;
	const float TRIGGERALPHA = 0.40f;
	private Color _debugColor;

	private Rectangle triggerHitbox;

	private bool previous_state = false;

	private readonly Scope? _onEnter;
	private readonly Scope? _onExit;
	private readonly Scope? _onStay;

	public Trigger(uint id, Rectangle hitbox, Scope? onEnter, Scope? onExit, Scope? onStay, Color color)
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
		if (_onEnter == null)
		{
			return;
		}

		ScopeProcessor.RunScope(_onEnter, $"onEnter of Trigger {id}", new Source(0, null, $"onEnter of Trigger {id}"));
	}

	private void RunOnExit()
	{
		if (_onExit == null)
		{
			return;
		}

		ScopeProcessor.RunScope(_onExit, $"onExit of Trigger {id}", new Source(0, null, $"onExit of Trigger {id}"));
	}

	private void RunOnStay()
	{
		if (_onStay == null)
		{
			return;
		}

		ScopeProcessor.RunScope(_onStay, $"onStay of Trigger {id}", new Source(0, null, $"onStay of Trigger {id}"), sync:true);
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.FillRectangle(triggerHitbox, _debugColor, 0);
	}
}