using System;
using System.Collections.Generic;
using AsepriteDotNet;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using StoryIso.Entities;
using StoryIso.Enums;
using StoryIso.Misc;
using StoryIso.Scenes;

namespace StoryIso.ECS;

public class CharacterSystem : EntityUpdateSystem
{
	private ComponentMapper<Character> _characterMapper;
	private ComponentMapper<Transform2> _transformMapper;
	private ComponentMapper<Animation> _animationMapper;
	private ComponentMapper<Texture2D> _texture2DMapper;

	static readonly Dictionary<string, Movement> _movements = [];
	static readonly Dictionary<string, bool> _visibilityChanges = [];
	static readonly Dictionary<string, RelativeVector2> _positionChanges = [];
	static readonly Dictionary<string, Direction> _directionChanges = [];

	const float DEFAULT_MOVEMENT_SPEED = 100f;
	const float MOVEMENT_THRESHOLD = 1f;

	public CharacterSystem() : base(Aspect.All(typeof(Character), typeof(Transform2)).One(typeof(Animation), typeof(Texture2D))) { }

	public override void Update(GameTime gameTime)
	{
		float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

		foreach (var entityId in ActiveEntities)
		{
			var character = _characterMapper.Get(entityId);
			var transform = _transformMapper.Get(entityId);

			if (_visibilityChanges.TryGetValue(character.Name, out var visibility))
			{
				character.Visible = visibility;
				_visibilityChanges.Remove(character.Name);
			}

			if (_positionChanges.TryGetValue(character.Name, out var position))
			{
				transform.Position = position.ToAbsolute(transform.Position);
				_positionChanges.Remove(character.Name);
			}

			if (_movements.TryGetValue(character.Name, out var move))
			{
				character.Movement = move.ToAbsolute(transform.Position);
				_movements.Remove(character.Name);
			}

			if (_directionChanges.TryGetValue(character.Name, out var direction))
			{
				character.Direction = direction;
				_directionChanges.Remove(character.Name);
			}

			UpdateAnimation(entityId);

			if (!character.Movement.HasValue)
			{
				continue;
			}

			Vector2 delta_movement = character.Movement.Value.movement - transform.Position;

			if (delta_movement.Length() <= MOVEMENT_THRESHOLD)
			{
				character.Movement = null;
				continue;
			}

			if (delta_movement.Y > 0)
			{
				character.Direction = Direction.Down;
			}
			else if (delta_movement.Y < 0)
			{
				character.Direction = Direction.Up;
			}
			else if (delta_movement.X < 0)
			{
				character.Direction = Direction.Left;
			}
			else if (delta_movement.X > 0)
			{
				character.Direction = Direction.Right;
			}

			Vector2 movement = delta_movement.NormalizedCopy() * 
								DEFAULT_MOVEMENT_SPEED * 
								character.Movement.Value.speed * 
								deltaTime;
			
			transform.Position += movement;
		}
	}

	private void UpdateAnimation(int entityId)
	{
		var animation = _animationMapper.Get(entityId);

		if (animation == null)
		{
			return;
		}

		var character = _characterMapper.Get(entityId);

		if (!character.Visible)
		{
			return;
		}

		if (character.Moving)
		{
			switch (character.Direction)
			{
				case Direction.Up:
					animation.SetAnimation("Moving Up");
					break;
				
				case Direction.Down:
					animation.SetAnimation("Moving Down");
					break;

				case Direction.Left:
					animation.SetAnimation("Moving Left");
					break;

				case Direction.Right:
					animation.SetAnimation("Moving Right");
					break;

				default:
					break;
			}
		}
		else
		{
			switch (character.Direction)
			{
				case Direction.Up:
					animation.SetAnimation("Standing Up");
					break;
				
				case Direction.Down:
					animation.SetAnimation("Standing Down");
					break;

				case Direction.Left:
					animation.SetAnimation("Standing Left");
					break;

				case Direction.Right:
					animation.SetAnimation("Standing Right");
					break;

				default:
					break;
			}
		}
	}

	public override void Initialize(IComponentMapperService mapperService)
	{
		_characterMapper = mapperService.GetMapper<Character>();
		_transformMapper = mapperService.GetMapper<Transform2>();
		_animationMapper = mapperService.GetMapper<Animation>();
		_texture2DMapper = mapperService.GetMapper<Texture2D>();
	}

	public static void SetCharacterPosition(string character, RelativeVector2 position)
	{
		_positionChanges[character.Trim('"')] = position;
	}

	public static void SetPlayerPosition(RelativeVector2 position)
	{
		SetCharacterPosition("Player", position);
	}

	public static void SetCharacterVisibility(string character, bool visibility)
	{
		_visibilityChanges[character.Trim('"')] = visibility;
	}

	public static void SetPlayerVisibility(bool visibility)
	{
		SetCharacterVisibility("Player", visibility);
	}

	public static void MoveCharacter(string character, Movement movement)
	{
		_movements[character.Trim('"')] = movement;
	}

	public static void MovePlayer(Movement movement)
	{
		MoveCharacter("Player", movement);
	}

	public static void SetCharacterDirection(string character, Direction direction)
	{
		_directionChanges[character.Trim('"')] = direction;
	}

	public static void SetPlayerDirection(Direction direction)
	{
		SetCharacterDirection("Player", direction);
	}
}