using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using StoryIso.Entities;
using StoryIso.Enums;
using StoryIso.Misc;

namespace StoryIso.ECS;

public class PlayerSystem : EntityProcessingSystem
{
	private ComponentMapper<Player> _playerMapper = null!;
	private ComponentMapper<Character> _characterMapper = null!;
	private ComponentMapper<Transform2> _transformMapper = null!;
	private ComponentMapper<Texture2D> _textureMapper = null!;
	private ComponentMapper<Animation> _animationMapper = null!;

	private static Vector2? _newPlayerPosition = null;
	private static float? _newPlayerX = null;
	private static float? _newPlayerY = null;
	private static Vector2? _deltaPlayerPosition = null;
	private static Vector2 _playerVelocity = Vector2.Zero;
	private static readonly System.Threading.Lock _positionLock = new();
	private static readonly System.Threading.Lock _xLock = new();
	private static readonly System.Threading.Lock _yLock = new();
	private static readonly System.Threading.Lock _deltaPositionLock = new();
	private static readonly System.Threading.Lock _velocityLock = new();

	private bool _collidingUp = false;

	public PlayerSystem() : base(Aspect.All(typeof(Player), typeof(Character), typeof(Transform2)).One(typeof(Texture2D), typeof(Animation))) { }

	public override void Initialize(IComponentMapperService mapperService)
	{
		_playerMapper = mapperService.GetMapper<Player>();
		_characterMapper = mapperService.GetMapper<Character>();
		_transformMapper = mapperService.GetMapper<Transform2>();
		_textureMapper = mapperService.GetMapper<Texture2D>();
		_animationMapper = mapperService.GetMapper<Animation>();
	}

	public override void Process(GameTime gameTime, int entityId)
	{
		var player = _playerMapper.Get(entityId);
		var character = _characterMapper.Get(entityId);
		var animation = _animationMapper.Get(entityId);
		var transform = _transformMapper.Get(entityId);
		var texture = _textureMapper.Get(entityId);

		float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

		lock (_velocityLock)
		{
			if (_playerVelocity != Vector2.Zero)
			{
				transform.Position += _playerVelocity * deltaTime;
				_playerVelocity = Vector2.Zero;
			}
		}

		lock (_deltaPositionLock)
		{
			if (_deltaPlayerPosition.HasValue)
			{
				transform.Position += _deltaPlayerPosition.Value;
				_deltaPlayerPosition = null;
			}
		}

		lock (_positionLock)
		{
			if (_newPlayerPosition.HasValue)
			{
				transform.Position = _newPlayerPosition.Value;
				_newPlayerPosition = null;
			}
		}

		lock (_xLock)
		{
			if (_newPlayerX.HasValue)
			{
				transform.Position = new Vector2(_newPlayerX.Value,  transform.Position.Y);
				_newPlayerX = null;
			}
		}

		lock (_yLock)
		{
			if (_newPlayerY.HasValue)
			{
				transform.Position = new Vector2(transform.Position.X, _newPlayerY.Value);
				_newPlayerY = null;
			}
		}

		var keystate = Keyboard.GetState();

		Vector2 movement = Vector2.Zero;
		character.Moving = false;

		if (!Game1.sceneManager.Active && !Game1.sceneManager.dialogueManager.Active) 
		{
			// the only thing _collidingUp does
			// is recreate the Frisk dance from Undertale
			if (keystate.IsKeyDown(Keys.Up) && !_collidingUp) 
			{
				movement.Y = -player.Speed * deltaTime;
				character.Moving = true;
				character.Direction = Direction.Up;
			}
			else if (keystate.IsKeyDown(Keys.Down))
			{
				movement.Y = player.Speed * deltaTime;
				character.Moving = true;
				character.Direction = Direction.Down;
			}
			else if (keystate.IsKeyDown(Keys.Up)) // more weird stuff to recreate frisk dance
			{
				character.Moving = true;
				character.Direction = Direction.Up;
			}

			if (keystate.IsKeyDown(Keys.Left))
			{
				movement.X = -player.Speed * deltaTime;

				if (!character.Moving) // prioritizes up and downward movements
				{
					character.Moving = true;
					character.Direction = Direction.Left;
				}
			}
			else if (keystate.IsKeyDown(Keys.Right))
			{
				movement.X = player.Speed * deltaTime;

				if (!character.Moving)
				{
					character.Moving = true;
					character.Direction = Direction.Right;
				}
			}
		}

		Move(entityId, movement);

		if (!Game1.sceneManager.dialogueManager.Active)
		{
			// Check triggers
			Rectangle player_hitbox;

			if (texture != null)
			{
				player_hitbox = new Rectangle(transform.Position.ToPoint(), (texture.Bounds.Size.ToVector2() * transform.Scale).ToPoint());
			}
			else
			{
				player_hitbox = new Rectangle(transform.Position.ToPoint(), (animation.GetFrame().Bounds.Size.ToVector2() * transform.Scale).ToPoint());
			}

			if (Game1.tiledManager.currentRoom == null)
			{
				return;
			}

			foreach (var trigger in Game1.tiledManager.currentRoom.Triggers)
			{
				trigger.CheckCollision(gameTime, player_hitbox);
			}

			foreach (var interaction in Game1.tiledManager.currentRoom.interactionTiles)
			{
				interaction.CheckInteract(player_hitbox, character.Direction);
			}
		}
	}

	public static void SetPlayerPosition(Vector2 new_position)
	{
		lock (_positionLock)
		{
			_newPlayerPosition = new_position;
		}
	}

	public static void SetPlayerX(float new_x)
	{
		lock (_xLock)
		{
			_newPlayerX = new_x;
		}
	}

	public static void SetPlayerY(float new_y)
	{
		lock (_yLock)
		{
			_newPlayerY = new_y;
		}
	}

	public static void SetPlayerPosition((RelativeVariable<float>, RelativeVariable<float>) position)
	{
		if (position.Item1.Relative && position.Item2.Relative)
		{
			MovePlayer(new Vector2(position.Item1.Value, position.Item2.Value));
			return;
		}

		if (position.Item1.Relative)
		{
			SetPlayerY(position.Item2.Value);
			MovePlayer(new Vector2(position.Item1.Value, 0));
			return;
		}

		if (position.Item2.Relative)
		{
			SetPlayerX(position.Item1.Value);
			MovePlayer(new Vector2(0, position.Item2.Value));
			return;
		}

		SetPlayerPosition(new Vector2(position.Item1.Value, position.Item2.Value));
	}

	public static void MovePlayer(Vector2 delta_position)
	{
		lock (_deltaPositionLock)
		{
			_deltaPlayerPosition = delta_position;
		}
	}

	public static void ApplyVelocity(Vector2 velocity)
	{
		lock (_velocityLock)
		{
			_playerVelocity += velocity;
		}
	}

	private void Move(int entityId, Vector2 movement)
	{
		if (movement == Vector2.Zero || Game1.tiledManager.currentRoom == null)
		{
			return;
		}

		const int LEVELOFDETAIL = 5; // how many rays to shoot out from each side (must be > 1!!)
		const float PADDING = 0.1f; // ray padding

		var transform = _transformMapper.Get(entityId);
		var texture = _textureMapper.Get(entityId);
		var animation = _animationMapper.Get(entityId);

		Point size = texture != null ? texture.Bounds.Size : animation.GetFrame().Bounds.Size;

		Ray2[]? horizontal_rays = null;
		if (movement.X != 0)
		{
			float start_x = transform.Position.X + (movement.X < 0 ? 0 : size.X * transform.Scale.X);

			horizontal_rays = new Ray2[LEVELOFDETAIL];

			for (int i = 0; i < LEVELOFDETAIL; i++)
			{
				float start_y = transform.Position.Y + PADDING + (size.Y - 2 * PADDING) * transform.Scale.Y * i / (LEVELOFDETAIL - 1);

				horizontal_rays[i] = new Ray2(new Vector2(start_x, start_y), new Vector2(movement.X, 0));
			}
		}

		Ray2[]? vertical_rays = null;
		if (movement.Y != 0)
		{
			float start_y = transform.Position.Y + (movement.Y < 0 ? 0 : size.Y * transform.Scale.Y);

			vertical_rays = new Ray2[LEVELOFDETAIL];

			for (int i = 0; i < LEVELOFDETAIL; i++)
			{
				float start_x = transform.Position.X + PADDING + (size.X - 2 * PADDING) * transform.Scale.X * i / (LEVELOFDETAIL - 1);

				vertical_rays[i] = new Ray2(new Vector2(start_x, start_y), new Vector2(0, movement.Y));
			}
		}

		foreach (var collider in Game1.tiledManager.currentRoom.Colliders)
		{
			if (!collider.Enabled)
			{
				continue;
			}

			BoundingRectangle boundingRect = collider.BoundingBox;

			// if the player is moving,
			// and it's actually possible to collide with the rect
			// then check the rays
			// efficiency :3
			if (movement.X != 0 && 
				collider.hitbox.Bottom > transform.Position.Y &&
				collider.hitbox.Top < transform.Position.Y + size.Y * transform.Scale.Y &&
				(movement.X < 0 || collider.hitbox.Right > transform.Position.X + size.X * transform.Scale.X) &&
				(movement.X > 0 || collider.hitbox.Left < transform.Position.X))
			{
				for (int i = 0; i < LEVELOFDETAIL; i++)
				{
					if (horizontal_rays![i].Intersects(boundingRect, out float near, out float _))
					{
						if (near < Math.Abs(movement.X))
						{
							movement.X = near * (movement.X < 0 ? -1 : 1);
						}
						break;
					}
				}
			}

			_collidingUp = false;
			if (movement.Y != 0 && 
				collider.hitbox.Left < transform.Position.X + size.X * transform.Scale.X &&
				collider.hitbox.Right > transform.Position.X &&
				(movement.Y < 0 || collider.hitbox.Bottom > transform.Position.Y + size.Y * transform.Scale.Y) &&
				(movement.Y > 0 || collider.hitbox.Top < transform.Position.Y))
			{
				for (int i = 0; i < LEVELOFDETAIL; i++)
				{
					if (vertical_rays![i].Intersects(boundingRect, out float near, out float _))
					{
						if (near < Math.Abs(movement.Y))
						{
							if (movement.Y < 0)
							{
								_collidingUp = true;
							}

							movement.Y = near * (movement.Y < 0 ? -1 : 1);
						}
						break;
					}
				}
			}

			if (movement == Vector2.Zero)
			{
				return;
			}
		}

		transform.Position += movement;
	}
}