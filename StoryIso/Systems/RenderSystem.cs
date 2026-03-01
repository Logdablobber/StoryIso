using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using MonoGame.Extended.Graphics;
using StoryIso.Entities;
using StoryIso.FileLoading;

namespace StoryIso.ECS;

public class RenderSystem : EntityDrawSystem
{
	private SpriteBatch _spriteBatch;
	private ComponentMapper<Texture2D> _textureMapper = null!;
	private ComponentMapper<Animation> _animationMapper = null!;
	private ComponentMapper<Transform2> _transformMapper = null!;
	private ComponentMapper<TextComponent> _textMapper = null!;
	private ComponentMapper<RenderAttributes> _renderAttributesMapper = null!;

	public RenderSystem(SpriteBatch spriteBatch)
		: base(Aspect.All(typeof(Transform2), typeof(RenderAttributes))
			.One(typeof(Texture2D), typeof(Animation), typeof(TextComponent)))
	{
		_spriteBatch = spriteBatch;
	}

	public override void Draw(GameTime gameTime)
	{
		_spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Game1.camera.GetViewMatrix());

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
				animation.GetFrame().Draw(_spriteBatch, draw_position, render_attributes.color, transform.Rotation, Vector2.Zero, draw_scale, SpriteEffects.None, render_attributes.ScreenElement ? 0f : 1f);
				continue;
			}

			Texture2D texture = _textureMapper.Get(entityId);
			if (texture != null)
			{
				_spriteBatch.Draw(texture, draw_position, null, render_attributes.color, transform.Rotation, Vector2.Zero, draw_scale, SpriteEffects.None, render_attributes.ScreenElement ? 0f : 1f);
				continue;
			}

			TextComponent text = _textMapper.Get(entityId); 
			if (text != null)
			{
				// TODO: Make this wrap
				BitmapFont? font = FontLoader.GetFont(text.FontName);

				if (font == null)
				{
					continue;
				}

				_spriteBatch.DrawString(font, text.Text, draw_position, render_attributes.color, transform.Rotation, Vector2.Zero, draw_scale, SpriteEffects.None, 0f);
			}			
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
		_textMapper = mapperService.GetMapper<TextComponent>();
		_renderAttributesMapper = mapperService.GetMapper<RenderAttributes>();
	}
}