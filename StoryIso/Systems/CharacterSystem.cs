using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using StoryIso.Debugging;
using StoryIso.Entities;
using StoryIso.Enums;
using StoryIso.Scripting;
using StoryIso.Misc;
using StoryIso.Scenes;
using StoryIso.UI;

namespace StoryIso.ECS;

public class CharacterSystem : EntityUpdateSystem
{
	private ComponentMapper<Character> _characterMapper = null!;
	private ComponentMapper<Transform2> _transformMapper = null!;
	private ComponentMapper<Animation> _animationMapper = null!;
	private ComponentMapper<Texture2D> _texture2DMapper = null!;
	private ComponentMapper<RenderAttributes> _renderAttributesMapper = null!;

	// this uses locks rather than concurrent dictionaries
	// as I do not want the dictionaries being changes
	// while being read, as the added values may be cleared and ignored
	// and that would be bad
	static readonly Dictionary<string, Movement> _movements = [];
	static readonly Dictionary<string, RelativeVector2> _positionChanges = [];
	static readonly Dictionary<string, List<(string, object)>> _attributeChanges = [];
	static readonly System.Threading.Lock _movementLock = new();
	static readonly System.Threading.Lock _positionChangesLock = new();
	static readonly System.Threading.Lock _attributeChangesLock = new();

	const float DEFAULT_MOVEMENT_SPEED = 100f;
	const float MOVEMENT_THRESHOLD = 1f;

	public CharacterSystem() : base(Aspect.All(typeof(Character), typeof(Transform2), typeof(RenderAttributes)).One(typeof(Animation), typeof(Texture2D))) { }

	public override void Update(GameTime gameTime)
	{
		float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

		foreach (var entityId in ActiveEntities)
		{
			var character = _characterMapper.Get(entityId);
			var transform = _transformMapper.Get(entityId);
			var render_attributes = _renderAttributesMapper.Get(entityId);

			lock (_attributeChangesLock)
			{
				if (_attributeChanges.TryGetValue(character.Name, out var attr_changes) && attr_changes.Count > 0)
				{
					foreach (var (attr, value) in attr_changes)
					{
						switch (attr.ToLower())
						{
							case "x":
								transform.Position.SetX(Game1.tiledManager.TileXToWorldX(((Optional<float>)value).Value));
								break;

							case "y":
								transform.Position.SetY(Game1.tiledManager.TileYToWorldY(((Optional<float>)value).Value));
								break;

							case "room":
								character.Room = ((Optional<string>)value).Value;
								break;

							case "visible":
								character.Visible = ((Optional<bool>)value).Value;
								break;

							case "direction":
								var direction = ParameterProcessor.GetDirection(((Optional<string>)value).Value);

								if (direction == Direction.None)
								{
									break;
								}

								character.Direction = direction;
								break;

							case "scale":
								transform.Scale = new Vector2(((Optional<float>)value).Value);
								break;

							case "speed":
								if (character.Name != "Player")
								{
									break;
								}

								Game1.player.Get<Player>().Speed = ((Optional<float>)value).Value;
								break;

							case "movement_locked":
								if (character.Name != "Player")
								{
									break;
								}

								Game1.sceneManager.Active = ((Optional<bool>)value).Value;
								break;

							default:
								throw new NotImplementedException();
						}
					}

					_attributeChanges[character.Name].Clear();
				}
			}

			lock (_positionChangesLock)
			{
				if (_positionChanges.TryGetValue(character.Name, out var position))
				{
					transform.Position = position.ToAbsolute(transform.Position);
					_positionChanges.Remove(character.Name);
				}
			}

			lock (_movementLock)
			{
				if (_movements.TryGetValue(character.Name, out var move))
				{
					character.Movement = move.ToAbsolute(transform.Position);
					_movements.Remove(character.Name);
				}
			}

			render_attributes.Visible = (character.Visible ?? true) && (character.Room == "#any#" || character.Room == Game1.tiledManager.currentRoomName);

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

		var render_attributes = _renderAttributesMapper.Get(entityId);

		if (!render_attributes.Visible)
		{
			return;
		}

		var character = _characterMapper.Get(entityId);
		
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
		_renderAttributesMapper = mapperService.GetMapper<RenderAttributes>();
	}

	public static void SetCharacterPosition(string character, RelativeVector2 position)
	{
		lock (_positionChangesLock)
		{
			_positionChanges[character.Trim('"')] = position;
		}
	}

	public static void SetPlayerPosition(RelativeVector2 position)
	{
		SetCharacterPosition("Player", position);
	}

	public static void MoveCharacter(string character, Movement movement)
	{
		lock (_movementLock)
		{
			_movements[character.Trim('"')] = movement;
		}
	}

	public static void MovePlayer(Movement movement)
	{
		MoveCharacter("Player", movement);
	}

	private static readonly Dictionary<string, Type> _universalAttributes = new () 
	{
		{"x", typeof(float)},
		{"y", typeof(float)},
		{"visible", typeof(bool)},
		{"scale", typeof(float)},
	};

	private static readonly Dictionary<string, Type> _uiOnlyAttributes = new()
	{
		{"text", typeof(string)},
	};

	private static readonly Dictionary<string, Type> _allCharacterAttributes = new() 
	{
		{"direction", typeof(string)},
	};

	private static readonly Dictionary<string, Type> _characterOnlyAttributes = new()
	{
		{"room", typeof(string)}
	};

	private static readonly Dictionary<string, Type> _playerOnlyAttributes = new()
	{
		{"speed", typeof(float)},
		{"movement_locked", typeof(bool)}
	};

	private static readonly Dictionary<string, Type> _uiAttributes = _universalAttributes.Union(_uiOnlyAttributes).ToDictionary();
	private static readonly Dictionary<string, Type> _playerAttributes = _universalAttributes.Union(_allCharacterAttributes).Union(_playerOnlyAttributes).ToDictionary();
	private static readonly Dictionary<string, Type> _characterAttributes = _universalAttributes.Union(_allCharacterAttributes).Union(_characterOnlyAttributes).ToDictionary();

	public static void SetAttribute(Source source, string target, string attribute, object value, Type type)
	{
		target = target.Trim('"');
		attribute = attribute.ToLower().Trim('"');

		if (UIManager.UIElements.Contains(target))
		{
			if (!_uiAttributes.TryGetValue(attribute, out var ui_attr_type))
			{
				DebugConsole.Raise(new UnknownVariableError(source, attribute, $"object '{target}' does not have attribute '{attribute}'"));
				return;
			}

			if (ui_attr_type == type)
			{
				UISystem.SetAttributeChange(target, attribute, value);
				return;
			}

			// convert between floats and ints
			if (ui_attr_type == typeof(int) && type == typeof(float))
			{
				UISystem.SetAttributeChange(target, attribute, new Optional<int>((int)((Optional<float>)value).Value));
				return;
			}

			if (ui_attr_type == typeof(float) && type == typeof(int))
			{
				UISystem.SetAttributeChange(target, attribute, new Optional<float>(((Optional<int>)value).Value));
				return;
			}

			DebugConsole.Raise(new WrongVariableTypeError(source, attribute, ui_attr_type.Name, $"Type of '{type.Name}' was inputted for attribute '{attribute}' of object '{target}'"));
			return;
		}
		
		var attributes_dict = target == "Player" ? _playerAttributes : _characterAttributes;

		if (attributes_dict.TryGetValue(attribute, out var attr_type))
		{
			if (attr_type == type)
			{
				SetAttributeChange(target, attribute, value);
				return;
			}

			// convert between floats and ints
			if (attr_type == typeof(int) && type == typeof(float))
			{
				SetAttributeChange(target, attribute, (int)(float)value);
				return;
			}

			if (attr_type == typeof(float) && type == typeof(int))
			{
				SetAttributeChange(target, attribute, (float)(int)value);
				return;
			}

			DebugConsole.Raise(new WrongVariableTypeError(source, attribute, attr_type.Name, $"Type of '{type.Name}' was inputted for attribute '{attribute}' of object '{target}'"));
			return;
		}

		DebugConsole.Raise(new UnknownVariableError(source, attribute, $"object '{target}' does not have attribute '{attribute}'"));
	}

	private static void SetAttributeChange(string character, string attribute, object value)
	{
		lock (_attributeChangesLock)
		{
			if (_attributeChanges.TryGetValue(character, out var attribute_changes))
			{
				attribute_changes.Add((attribute, value));

				_attributeChanges[character] = attribute_changes;
				return;
			}

			_attributeChanges[character] = [(attribute, value)];
		}
	}
}