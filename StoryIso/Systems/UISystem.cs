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
using StoryIso.Debugging;
using StoryIso.Entities;
using StoryIso.Misc;

namespace StoryIso.ECS;

public class UISystem : EntityUpdateSystem
{
	private ComponentMapper<TextComponent> _textComponentMapper = null!;
	private ComponentMapper<UIInfo> _infoMapper = null!;

	private static readonly Dictionary<string, List<(string, IOptional)>> _attributeChanges = [];
	private static readonly System.Threading.Lock _attributeChangesLock = new();
    
    private static readonly Dictionary<string, List<(string, int)>> _attributeRetrievals = [];
    private static readonly ConcurrentDictionary<int, IOptional> _retrievedAttributes = [];
    private static readonly System.Threading.Lock _attributeRetrievalLock = new();

	public UISystem() : base(Aspect.All(typeof(RenderAttributes), typeof(UIInfo))
									.One(typeof(TextComponent), typeof(Texture2D), typeof(Animation), typeof(RectangleComponent), typeof(PolygonComponent))) { }

	public override void Update(GameTime gameTime)
	{
		foreach (var entityId in ActiveEntities)
		{
			UpdateAttributes(entityId);
            RetrieveAttributes(entityId);
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

					case "x":
						info.SetX(((Optional<float>)attribute.Item2).Value);
						break;

					case "y":
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

				_attributeChanges[info.Name].Clear();
			}
		}
	}
    
    private void RetrieveAttributes(int entityId)
    {
	    var text_component = _textComponentMapper.Get(entityId);
	    var info = _infoMapper.Get(entityId);
        
        lock (_attributeRetrievalLock)
        {
	        if (!_attributeRetrievals.TryGetValue(info.Name, out var attributes) || attributes.Count == 0)
	        {
		        return;
	        }

	        foreach (var (attribute, id) in attributes)
	        {
				_retrievedAttributes[id] = attribute.ToLower() switch
				{
					"visible" => new Optional<bool>(info.Visible),
					"x" => new Optional<float>(info.LocalPosition.X),
					"y" => new Optional<float>(info.LocalPosition.Y),
					"scale" => new Optional<float>(info.Scale.X),
					"text" when text_component != null => new Optional<string>(text_component.Text),
					_ => throw new NotImplementedException(),
				};
                
				ThreadManager.Send(id);
	        }
            
            _attributeChanges.Clear();
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
		var id = Environment.CurrentManagedThreadId;

		lock (_attributeRetrievalLock)
		{
			if (_attributeRetrievals.TryGetValue(target, out var ids))
			{
				ids.Add((attribute, id));
			}
			else
			{
				_attributeRetrievals[target] = [(attribute, id)];
			}
		}

		ThreadManager.Await(id);

		return _retrievedAttributes[id];
	}
}