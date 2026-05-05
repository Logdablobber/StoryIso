using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using Entropy.Debugging;
using Entropy.Entities;
using Entropy.Misc;
using Entropy.UI;

namespace Entropy.ECS;

public class UISystem : EntityUpdateSystem
{
	private ComponentMapper<TextComponent> _textComponentMapper = null!;
	private ComponentMapper<UIInfo> _infoMapper = null!;

	private static readonly Dictionary<string, List<(string, IOptional)>> _attributeChanges = [];
	private static readonly System.Threading.Lock _attributeChangesLock = new();

    private static readonly Dictionary<string, int> _uiEntities = [];

	public UISystem() : base(Aspect.All(typeof(UIInfo))) { }

	public override void Update(GameTime gameTime)
	{
		foreach (var entityId in ActiveEntities)
		{
			UpdateAttributes(entityId);
		}
	}

	private void UpdateAttributes(int entityId)
	{
		var text_component = _textComponentMapper.Get(entityId);
		var info = _infoMapper.Get(entityId);

		lock (_attributeChangesLock)
		{
			if (!_attributeChanges.TryGetValue(info.Name, out var attributes) || attributes.Count == 0)
			{
				return;
			}
            
			foreach (var attribute in attributes)
			{
				switch (attribute.Item1.ToLower())
				{
					case "visible":
						info.Visible = ((Optional<bool>)attribute.Item2).Value;
						break;

					// TODO: make x and y set the absolute position
					case "x":
                    case "local_x":
						info.SetX(((Optional<float>)attribute.Item2).Value);
						break;

					case "y":
                    case "local_y":
						info.SetY(((Optional<float>)attribute.Item2).Value);
						break;

					case "scale":
						info.Scale = new Vector2(((Optional<float>)attribute.Item2).Value);
						break;

					case "text" when text_component != null:
						text_component.SetText(((Optional<string>)attribute.Item2).Value);
						break;

					default:
						throw new NotImplementedException();
				}				
			}
            
            _attributeChanges[info.Name].Clear();
		}
	}
    
	public override void Initialize(IComponentMapperService mapperService)
	{
		_textComponentMapper = mapperService.GetMapper<TextComponent>();
		_infoMapper = mapperService.GetMapper<UIInfo>();
	}

	public static void SetAttributeChange(string target, string attribute, IOptional value) 
	{
		lock (_attributeChangesLock) 
		{
			if (_attributeChanges.TryGetValue(target, out var changes))
			{
				changes.Add((attribute, value));
				_attributeChanges[target] = changes;
				return;
			}

			_attributeChanges.Add(target, [(attribute, value)]);
		}
	}

	public static IOptional GetAttribute(string target, string attribute)
	{
		if (!_uiEntities.TryGetValue(target, out var entityId))
		{
			return new Optional<string>();
		}

		var entity = Game1.world.GetEntity(entityId);

		switch (attribute)
		{
			case "visible":
				var info1 = entity.Get<UIInfo>();

				return new Optional<bool>(info1.Visible);
            
            case "x":
				var info2 = entity.Get<UIInfo>();

				return new Optional<float>(info2.Position.X);
            
            case "y":
				var info3 = entity.Get<UIInfo>();

				return new Optional<float>(info3.Position.X);
            
            case "local_x":
				var info4 = entity.Get<UIInfo>();

				return new Optional<float>(info4.LocalPosition.X);

			case "local_y":
				var info5 = entity.Get<UIInfo>();

				return new Optional<float>(info5.LocalPosition.Y);
            
            case "scale":
				var info6 = entity.Get<UIInfo>();

				return new Optional<float>(info6.Scale.X);
            
            case "text":
				var text = entity.Get<TextComponent>();
                
                if (text == null)
                {
	                return new Optional<string>();
                }

				return new Optional<string>(text.Text);
            
			default:
				throw new NotImplementedException();
		}
	}

	protected override void OnEntityAdded(int entityId)
	{
		base.OnEntityAdded(entityId);

		if (!_infoMapper.TryGet(entityId, out var info))
		{
			return;
		}
        
        _uiEntities.Add(info.Name, entityId);
	}
}