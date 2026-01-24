using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using MonoGame.Extended.Graphics;
using StoryIso.Entities;

namespace StoryIso.ECS;

public class RenderSystem : EntityDrawSystem
{
	private SpriteBatch _spriteBatch;
	private ComponentMapper<Texture2D> _textureMapper;
	private ComponentMapper<Animation> _animationMapper;
	private ComponentMapper<Transform2> _transformMapper;

	public RenderSystem(SpriteBatch spriteBatch)
		: base(Aspect.All(typeof(Transform2)).One(typeof(Texture2D), typeof(Animation)))
	{
		_spriteBatch = spriteBatch;
	}

	public override void Draw(GameTime gameTime)
	{
		_spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: Game1.camera.GetViewMatrix());

		foreach (var entityId in ActiveEntities)
		{
			var transform = _transformMapper.Get(entityId);

			Texture2D texture = _textureMapper.Get(entityId);
			if (texture != null)
			{
				_spriteBatch.Draw(texture, transform.Position, null, Color.White, transform.Rotation, Vector2.Zero, transform.Scale, SpriteEffects.None, 0f);
				continue;
			}

			Animation animation = _animationMapper.Get(entityId);
			animation.GetFrame().Draw(_spriteBatch, transform.Position, Color.White, transform.Rotation, Vector2.Zero, transform.Scale, SpriteEffects.None, 0f);
		}

		_spriteBatch.End();
	}

	public override void Initialize(IComponentMapperService mapperService)
	{
		_textureMapper = mapperService.GetMapper<Texture2D>();
		_transformMapper = mapperService.GetMapper<Transform2>();
		_animationMapper = mapperService.GetMapper<Animation>();
	}
}