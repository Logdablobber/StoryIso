using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using Entropy.Debugging;
using Entropy.Entities;
using Entropy.Enums;
using Entropy.Scripting;
using Entropy.Misc;
using Entropy.Scenes;
using Entropy.UI;
using Entropy.Scripting.Variables;

namespace Entropy.ECS;

public class CharacterSystem : EntityUpdateSystem
{
	private ComponentMapper<Character> _characterMapper = null!;
	private ComponentMapper<Transform2> _transformMapper = null!;
	private ComponentMapper<Animation> _animationMapper = null!;
	private ComponentMapper<Texture2D> _texture2DMapper = null!;
	private ComponentMapper<RenderAttributes> _renderAttributesMapper = null!;

	// this uses locks rather than concurrent dictionaries
	// as I do not want the dictionaries being changed
	// while being read, as the added values may be cleared and ignored
	// and that would be bad
	private static readonly Dictionary<string, Movement> _movements = [];
	private static readonly Dictionary<string, RelativeVector2> _positionChanges = [];
	private static readonly Dictionary<string, List<(string, IOptional)>> _attributeChanges = [];
	private static readonly System.Threading.Lock _movementLock = new();
	private static readonly System.Threading.Lock _positionChangesLock = new();
	private static readonly System.Threading.Lock _attributeChangesLock = new();
    
    private static readonly Dictionary<string, int> _characterEntities = [];

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

			UpdateAttributes(entityId);

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

			character.Direction = delta_movement.Y switch
			{
				> 0 => Direction.Down,
				< 0 => Direction.Up,
				_ => delta_movement.X switch
				{
					< 0 => Direction.Left,
					> 0 => Direction.Right,
					_ => character.Direction
				}
			};

			Vector2 movement = delta_movement.NormalizedCopy() * 
			                   DEFAULT_MOVEMENT_SPEED * 
			                   character.Movement.Value.speed * 
			                   deltaTime;
			
			transform.Position += movement;
		}
	}
    
    private void UpdateAttributes(int entityId)
    {
	    var character = _characterMapper.Get(entityId);
	    var transform = _transformMapper.Get(entityId);

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
        
		lock (_attributeChangesLock)
		{
			if (!_attributeChanges.TryGetValue(character.Name, out var attr_changes) || attr_changes.Count == 0)
			{
				return;
			}
            
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

					case "speed" when character.Name == "Player":
						Game1.player.Get<Player>().Speed = ((Optional<float>)value).Value;
						break;

					case "movement_locked" when character.Name == "Player":
						Game1.sceneManager.Active = ((Optional<bool>)value).Value;
						break;

					default:
						throw new NotImplementedException();
				}
			}
            
            _attributeChanges[character.Name].Clear();
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
                
				case Direction.None:
					break;
                
				default:
					throw new ArgumentOutOfRangeException();
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

				case Direction.None:
					break;

				default:
					throw new ArgumentOutOfRangeException();
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
		{"local_x", typeof(float)},
		{"local_y", typeof(float)}
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

	public static void SetAttribute(Source source, string target, string attribute, IOptional value)
	{
		if (!value.HasValue)
		{
			return;
		}

		attribute = attribute.ToLower();

		if (UIManager.UIElements.Contains(target))
		{
			if (!_uiAttributes.TryGetValue(attribute, out var ui_attr_type))
			{
				DebugConsole.Raise(new UnknownVariableError(source, attribute, $"object '{target}' does not have attribute '{attribute}'"));
				return;
			}

			IOptional converted_value = ParameterProcessor.ConvertOptional(source, value, VariableManager.GetVariableType(ui_attr_type));

			if (!converted_value.HasValue) 
			{
				DebugConsole.Raise(new WrongVariableTypeError(source, attribute, ui_attr_type.Name, $"Type of '{value.ValueType.Name}' was inputted for attribute '{attribute}' of object '{target}'"));
			}

			UISystem.SetAttributeChange(target, attribute, converted_value);
			return;
		}
		
		var attributes_dict = target == "Player" ? _playerAttributes : _characterAttributes;

		if (attributes_dict.TryGetValue(attribute, out var attr_type))
		{
			IOptional converted_value = ParameterProcessor.ConvertOptional(source, value, VariableManager.GetVariableType(attr_type));

			if (!converted_value.HasValue) 
			{
				DebugConsole.Raise(new WrongVariableTypeError(source, attribute, attr_type.Name, $"Type of '{value.ValueType.Name}' was inputted for attribute '{attribute}' of object '{target}'"));
			}

			SetAttributeChange(target, attribute, converted_value);
			return;
		}

		DebugConsole.Raise(new UnknownVariableError(source, attribute, $"object '{target}' does not have attribute '{attribute}'"));
	}

	public static IOptional GetAttribute(Source source, string target, string attribute)
	{
		attribute = attribute.ToLower();

		if (UIManager.UIElements.Contains(target))
		{
			if (_uiAttributes.ContainsKey(attribute))
			{
				return UISystem.GetAttribute(target, attribute);
			}
            
			DebugConsole.Raise(new UnknownVariableError(source, attribute, $"object '{target}' does not have attribute '{attribute}'"));
			return new Optional<string>();
		}

		var attributes_dict = target == "Player" ? _playerAttributes : _characterAttributes;

		if (attributes_dict.ContainsKey(attribute))
		{
			return GetAttribute(target, attribute);
		}

		DebugConsole.Raise(new UnknownVariableError(source, attribute,
			$"object '{target}' does not have attribute '{attribute}'"));

		return new Optional<string>();
	}
    
    private static IOptional GetAttribute(string character, string attribute)
    {
	    if (!_characterEntities.TryGetValue(character, out var entityId))
	    {
		    return new Optional<string>();
	    }

	    var entity = Game1.world.GetEntity(entityId);
        
        switch (attribute)
		{
			case "x":
				var transform1 = entity.Get<Transform2>();

				return new Optional<float>(transform1.Position.X);
            
            case "y":
				var transform2 = entity.Get<Transform2>();

				return new Optional<float>(transform2.Position.Y);

			case "scale":
				var transform3 = entity.Get<Transform2>();

				return new Optional<float>(transform3.Scale.X);
            
            case "room":
				var char1 = entity.Get<Character>();

				return new Optional<string>(char1.Room);
            
            case "visible":
				var char2 = entity.Get<Character>();

				return new Optional<bool>(char2.Visible ?? false);
            
            case "direction":
				var char3 = entity.Get<Character>();

				return new Optional<string>(char3.Direction.ToString());
            
            case "speed":
				var char4 = entity.Get<Character>();
                
                if (char4.Name != "player")
                {
	                return new Optional<float>();
                }

                var player = entity.Get<Player>();

                return new Optional<float>(player.Speed);
            
            case "movementlocked":
				var char5 = entity.Get<Character>();

				if (char5.Name != "player")
				{
					return new Optional<float>();
				}

				return new Optional<bool>(Game1.sceneManager.Active);
            
            default:
				throw new NotImplementedException();
 		}
    }

	private static void SetAttributeChange(string character, string attribute, IOptional value)
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

	protected override void OnEntityAdded(int entityId)
	{        
		base.OnEntityAdded(entityId);
        
        if (!_characterMapper.TryGet(entityId, out var character))
        {
	        return;
        }
        
        _characterEntities.Add(character.Name, entityId);
	}
}