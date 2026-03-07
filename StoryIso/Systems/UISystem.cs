using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using StoryIso.Debugging;
using StoryIso.Entities;
using StoryIso.Misc;
using StoryIso.UI;

namespace StoryIso.ECS;

public class UISystem : EntityUpdateSystem
{
	private ComponentMapper<Transform2> _transformMapper = null!;
	private ComponentMapper<RenderAttributes> _renderAttributesMapper = null!;
	private ComponentMapper<TextComponent> _textComponentMapper = null!;
	private ComponentMapper<UIInfo> _infoMapper = null!;

	private static readonly Dictionary<string, List<(string, object)>> _attributeChanges = [];
	private static readonly System.Threading.Lock _attributeChangesLock = new();

	public UISystem() : base(Aspect.All(typeof(Transform2), typeof(RenderAttributes), typeof(UIInfo))
									.One(typeof(TextComponent), typeof(Texture2D), typeof(Animation))) { }

	public override void Update(GameTime gameTime)
	{
		foreach (var entityId in ActiveEntities)
		{
			var transform = _transformMapper.Get(entityId);
			var render_attributes = _renderAttributesMapper.Get(entityId);
			var text_component = _textComponentMapper.Get(entityId);
			var info = _infoMapper.Get(entityId);

			lock (_attributeChangesLock)
			{
				if (info.Parent != null && _attributeChanges.TryGetValue(info.Parent.Name, out var parent_attributes) && parent_attributes.Count != 0)
				{
					foreach (var attribute in parent_attributes)
					{
						switch (attribute.Item1.ToLower())
						{
							case "visible":
								UIManager.SetObjectVisible(info.Parent.Name, ((Optional<bool>)attribute.Item2).Value);
								break;

							case "x":
								UIManager.SetObjectX(info.Parent.Name, ((Optional<float>)attribute.Item2).Value);
								break;

							case "y":
								UIManager.SetObjectY(info.Parent.Name, ((Optional<float>)attribute.Item2).Value);
								break;

							case "scale":
								UIManager.SetObjectScale(info.Parent.Name, ((Optional<float>)attribute.Item2).Value);
								break;

							case "text":
								DebugConsole.Raise(new UnknownVariableError(new Source(0, "SetAttr", info.Parent.Name), attribute.Item1, $"object '{info.Parent.Name}' does not have attribute '{attribute}'"));
								break;

							default:
								throw new NotImplementedException();
						}
					}

					_attributeChanges[info.Parent.Name].Clear();
				}

				if (_attributeChanges.TryGetValue(info.Name, out var attributes) && attributes.Count != 0)
				{
					foreach (var attribute in attributes)
					{
						switch (attribute.Item1.ToLower())
						{
							case "visible":
								info.Visible = ((Optional<bool>)attribute.Item2).Value;
								break;

							case "x":
								info.SetX(((Optional<float>)attribute.Item2).Value);
								break;

							case "y":
								info.SetY(((Optional<float>)attribute.Item2).Value);
								break;

							case "scale":
								info.Scale = new Vector2(((Optional<float>)attribute.Item2).Value);
								break;

							case "text":
								if (text_component == null)
								{
									DebugConsole.Raise(new UnknownVariableError(new Source(0, "SetAttr", info.Name), attribute.Item1, $"object '{info.Name}' does not have attribute '{attribute}'"));
									break;
								}

								text_component.SetText(((Optional<string>)attribute.Item2).Value);
								break;

							default:
								throw new NotImplementedException();
						}
					}

					_attributeChanges[info.Name].Clear();
				}
			}

			render_attributes.Visible = info.Visible;
			transform.Position = info.Position;
			transform.Scale = info.Scale;
		}
	}

	public override void Initialize(IComponentMapperService mapperService)
	{
		_transformMapper = mapperService.GetMapper<Transform2>();
		_renderAttributesMapper = mapperService.GetMapper<RenderAttributes>();
		_textComponentMapper = mapperService.GetMapper<TextComponent>();
		_infoMapper = mapperService.GetMapper<UIInfo>();
	}

	public static void SetAttributeChange(string target, string attribute, object value) 
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
}