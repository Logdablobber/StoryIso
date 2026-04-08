using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using StoryIso.Debugging;
using StoryIso.Entities;
using StoryIso.Scenes;
using StoryIso.UI;

namespace StoryIso.ECS;

public class ButtonSystem : EntityUpdateSystem
{
	private ComponentMapper<ButtonComponent> _buttonMapper = null!;
	private ComponentMapper<UIInfo> _uiMapper = null!;

	private MouseState? _previousMouseState = null;

	public ButtonSystem() : base(Aspect.All(typeof(ButtonComponent), typeof(UIInfo))) {}

	public override void Update(GameTime gameTime)
	{
		MouseState state = Mouse.GetState();

		List<Task> tasks = [];

		foreach (var entityID in ActiveEntities)
		{
			UIInfo info = _uiMapper.Get(entityID);
			ButtonComponent button = _buttonMapper.Get(entityID);

			Task new_task = new(() =>
			{
				Process(info.Position, info.Scale, button, state, _previousMouseState);
			});
			new_task.Start();

			tasks.Add(new_task);
		}

		Task.WaitAll(tasks);

		_previousMouseState = state;
	}

	private void Process(Vector2 position, Vector2 scale, ButtonComponent button, MouseState current_state, MouseState? previous_state)
	{
		Source source = new(0, null, button.name);

		RectangleF hitbox = new RectangleF(position, button.size * scale);

		// TODO: figure out why the hitbox won't line up correctly

		if (hitbox.Contains(current_state.Position.ToVector2()))
		{
			if (button.onStay != null)
			{
				Game1.sceneManager.RunScene(button.onStay, source);
			}

			if (button.onEnter != null && (!previous_state.HasValue || hitbox.Contains(previous_state.Value.Position.ToVector2())))
			{
				Game1.sceneManager.RunScene(button.onEnter, source);
			}

			if (current_state.LeftButton == ButtonState.Pressed)
			{
				if (button.whileLeftHeld != null)
				{
					Game1.sceneManager.RunScene(button.whileLeftHeld, source);
				}

				button.left_held = true;

				if (button.onLeftClick != null && (!previous_state.HasValue || previous_state.Value.LeftButton == ButtonState.Released))
				{
					Game1.sceneManager.RunScene(button.onLeftClick, source);
				}
			}
			else if (previous_state.HasValue && previous_state.Value.LeftButton == ButtonState.Pressed)
			{
				// should only run release if button was clicked first
				if (button.left_held && button.onLeftRelease != null)
				{
					Game1.sceneManager.RunScene(button.onLeftRelease, source);
				}

				button.left_held = false;
			}

			if (current_state.RightButton == ButtonState.Pressed)
			{
				if (button.whileRightHeld != null)
				{
					Game1.sceneManager.RunScene(button.whileRightHeld, source);
				}

				button.right_held = true;

				if (button.onRightClick != null && (!previous_state.HasValue || previous_state.Value.RightButton == ButtonState.Released))
				{
					Game1.sceneManager.RunScene(button.onRightClick, source);
				}
			}
			else if (previous_state.HasValue && previous_state.Value.RightButton == ButtonState.Pressed)
			{
				// should only run release if button was clicked first
				if (button.right_held && button.onRightRelease != null)
				{
					Game1.sceneManager.RunScene(button.onRightRelease, source);
				}

				button.right_held = false;
			}

			return;
		}

		if (!previous_state.HasValue)
		{
			return;
		}

		if (hitbox.Contains(previous_state.Value.Position.ToVector2()))
		{
			if (button.onExit != null)
			{
				Game1.sceneManager.RunScene(button.onExit, source);
			}

			if (button.left_held)
			{
				if (button.onLeftRelease != null)
				{
					Game1.sceneManager.RunScene(button.onLeftRelease, source);
				}
				button.left_held = false;
			}

			if (button.right_held)
			{
				if (button.onRightRelease != null)
				{
					Game1.sceneManager.RunScene(button.onRightRelease, source);
				}
				button.right_held = false;
			}
		}
	}

	public override void Initialize(IComponentMapperService mapperService)
	{
		_buttonMapper = mapperService.GetMapper<ButtonComponent>();
		_uiMapper = mapperService.GetMapper<UIInfo>();
	}
}