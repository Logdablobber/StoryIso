using Microsoft.Xna.Framework;
using MonoGame.Extended.ECS;
using MonoGame.Extended.ECS.Systems;
using StoryIso.Entities;

namespace StoryIso.ECS;

public class AnimationSystem : EntityUpdateSystem
{
	private ComponentMapper<Animation> _animationMapper;

	public AnimationSystem() : base(Aspect.All(typeof(Animation))) { }

	public override void Update(GameTime gameTime)
	{
		foreach (var entityId in ActiveEntities)
		{
			var animation = _animationMapper.Get(entityId);

			animation.Update(gameTime);
		}
	}

	public override void Initialize(IComponentMapperService mapperService)
	{
		_animationMapper = mapperService.GetMapper<Animation>();
	}
}