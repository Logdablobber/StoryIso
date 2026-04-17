using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Collisions.Layers;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using MonoGame.Extended.Shapes;
using StoryIso.Entities;
using StoryIso.Enums;
using StoryIso.FileLoading;

namespace StoryIso.ECS;

public class UIRenderSystem : EntityDrawSystem
{
	private readonly SpriteBatch _spriteBatch;
	private ComponentMapper<Texture2D> _textureMapper = null!;
	private ComponentMapper<Animation> _animationMapper = null!;
	private ComponentMapper<UIInfo> _infoMapper = null!;
	private ComponentMapper<TextComponent> _textMapper = null!;
	private ComponentMapper<RectangleComponent> _rectMapper = null!;
	private ComponentMapper<PolygonComponent> _polyMapper = null!;
	private ComponentMapper<RenderAttributes> _renderAttributesMapper = null!;

	public UIRenderSystem(SpriteBatch spriteBatch)
		: base(Aspect.All(typeof(RenderAttributes), typeof(UIInfo))
			.One(typeof(Texture2D), typeof(Animation), typeof(TextComponent), typeof(RectangleComponent), typeof(PolygonComponent)))
	{
		_spriteBatch = spriteBatch;
	}

	public override void Draw(GameTime gameTime)
	{
		_spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Game1.camera.GetViewMatrix(), sortMode: SpriteSortMode.Deferred);

		foreach (var entityId in ActiveEntities)
		{
			var info = _infoMapper.Get(entityId);
		
			if (!info.Visible)
			{
				continue;
			}

			var render_attributes = _renderAttributesMapper.Get(entityId);

			GetScreenTransform(render_attributes.ScreenElement, info.Position, info.Scale, 
								out Vector2 draw_position,
								out Vector2 draw_scale);

			Animation animation = _animationMapper.Get(entityId);
			if (animation != null)
			{
				var origin = render_attributes.GetOrigin(animation.GetFrame().Bounds.Size.ToVector2());
				animation.GetFrame().Draw(_spriteBatch, draw_position, render_attributes.color, 0, origin, draw_scale, SpriteEffects.None, render_attributes.renderLayer.GetLayerDepth());
				continue;
			}

			Texture2D texture = _textureMapper.Get(entityId);
			if (texture != null)
			{
				var origin = render_attributes.GetOrigin(texture.Bounds.Size.ToVector2());
				_spriteBatch.Draw(texture, draw_position, null, render_attributes.color, 0, origin, draw_scale, SpriteEffects.None, render_attributes.renderLayer.GetLayerDepth());
				continue;
			}

			TextComponent text = _textMapper.Get(entityId); 
			if (text != null)
			{
				// TODO: implement origin
				text.Draw(_spriteBatch, render_attributes.color, draw_position, draw_scale, 0, render_attributes.renderLayer.GetLayerDepth());
				continue;
			}

			RectangleComponent rect = _rectMapper.Get(entityId);
			if (rect != null)
			{
				// TODO: implement origin
				rect.Draw(_spriteBatch, render_attributes.color, draw_position, draw_scale, render_attributes.renderLayer.GetLayerDepth());
				continue;
			}

			PolygonComponent polygon = _polyMapper.Get(entityId);
			if (polygon != null)
			{
				polygon.Draw(render_attributes.color, info.Position, info.Scale, render_attributes.renderLayer.GetLayerDepth());
				continue;
			}

			throw new UnreachableException();
		}

		_spriteBatch.End();
	}

	private void GetScreenTransform(bool screen_element, 
									Vector2 position, 
									Vector2 scale, 
									out Vector2 screen_position, 
									out Vector2 screen_scale)
	{
		if (!screen_element)
		{
			screen_position = position;
			screen_scale = scale;
			return;
		}

		if (position.X < 0)
		{
			position.X = Game1.ScreenWidth + position.X;
		}

		if (position.Y < 0)
		{
			position.Y = Game1.ScreenHeight + position.Y;
		}

		screen_position = Game1.camera.ScreenToWorld(position);
		screen_scale = scale / Game1.camera.Zoom;
	}

	public override void Initialize(IComponentMapperService mapperService)
	{
		_textureMapper = mapperService.GetMapper<Texture2D>();
		_infoMapper = mapperService.GetMapper<UIInfo>();
		_animationMapper = mapperService.GetMapper<Animation>();
		_textMapper = mapperService.GetMapper<TextComponent>();
		_renderAttributesMapper = mapperService.GetMapper<RenderAttributes>();
		_rectMapper = mapperService.GetMapper<RectangleComponent>();
		_polyMapper = mapperService.GetMapper<PolygonComponent>();
	}
}