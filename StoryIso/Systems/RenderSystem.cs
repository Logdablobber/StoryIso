using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using MonoGame.Extended.Shapes;
using StoryIso.Entities;
using StoryIso.Enums;
using StoryIso.FileLoading;

namespace StoryIso.ECS;

public class RenderSystem : EntityDrawSystem
{
	private readonly SpriteBatch _spriteBatch;
	private ComponentMapper<Texture2D> _textureMapper = null!;
	private ComponentMapper<Animation> _animationMapper = null!;
	private ComponentMapper<Transform2> _transformMapper = null!;
	private ComponentMapper<RenderAttributes> _renderAttributesMapper = null!;

	public RenderSystem(SpriteBatch spriteBatch)
		: base(Aspect.All(typeof(Transform2), typeof(RenderAttributes))
			.One(typeof(Texture2D), typeof(Animation))
			.Exclude(typeof(UIInfo)))
	{
		_spriteBatch = spriteBatch;
	}

	public override void Draw(GameTime gameTime)
	{
		_spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Game1.camera.GetViewMatrix(), sortMode:SpriteSortMode.BackToFront);

		foreach (var entityId in ActiveEntities)
		{
			var render_attributes = _renderAttributesMapper.Get(entityId);

			if (!render_attributes.Visible)
			{
				continue;
			}

			var transform = _transformMapper.Get(entityId);

			GetScreenTransform(render_attributes.ScreenElement, transform.Position, transform.Scale, 
								out Vector2 draw_position,
								out Vector2 draw_scale);

			Animation animation = _animationMapper.Get(entityId);
			if (animation != null)
			{
				var origin = render_attributes.GetOrigin(animation.GetFrame().Bounds.Size.ToVector2());
				animation.GetFrame().Draw(_spriteBatch, draw_position, render_attributes.color, transform.Rotation, origin, draw_scale, SpriteEffects.None, render_attributes.renderLayer.GetLayerDepth());
				continue;
			}

			Texture2D texture = _textureMapper.Get(entityId);
			if (texture != null)
			{
				var origin = render_attributes.GetOrigin(texture.Bounds.Size.ToVector2());
				_spriteBatch.Draw(texture, draw_position, null, render_attributes.color, transform.Rotation, origin, draw_scale, SpriteEffects.None, render_attributes.renderLayer.GetLayerDepth());
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
		_transformMapper = mapperService.GetMapper<Transform2>();
		_animationMapper = mapperService.GetMapper<Animation>();
		_renderAttributesMapper = mapperService.GetMapper<RenderAttributes>();
	}
}