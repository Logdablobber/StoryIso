using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using StoryIso.Entities;
using StoryIso.UI;

namespace StoryIso.ECS;

public class UISystem : EntityUpdateSystem
{
	private ComponentMapper<Transform2> _transformMapper = null!;
	private ComponentMapper<RenderAttributes> _renderAttributesMapper = null!;
	private ComponentMapper<TextComponent> _textComponentMapper = null!;

	public UISystem() : base(Aspect.All(typeof(Transform2), typeof(RenderAttributes))
									.One(typeof(TextComponent), typeof(Texture2D), typeof(Animation))) { }

	public override void Update(GameTime gameTime)
	{
		foreach (var entityId in ActiveEntities)
		{
			var transform = _transformMapper.Get(entityId);
			var render_attributes = _renderAttributesMapper.Get(entityId);
		}
	}

	public override void Initialize(IComponentMapperService mapperService)
	{
		_transformMapper = mapperService.GetMapper<Transform2>();
		_renderAttributesMapper = mapperService.GetMapper<RenderAttributes>();
		_textComponentMapper = mapperService.GetMapper<TextComponent>();
	}
}