using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Aseprite;

namespace Entropy.Entities;

public class Animation
{
	readonly SpriteSheet _spriteSheet;
	readonly Dictionary<string, AnimatedSprite> _animations;
	string _currentAnimation;
	string? _overrideAnimation;


	public Animation(SpriteSheet sprite_sheet, string starting_animation)
	{
		_spriteSheet = sprite_sheet;
		_currentAnimation = starting_animation;
		_overrideAnimation = null;

		_animations = new Dictionary<string, AnimatedSprite>();
		foreach (var animation in _spriteSheet.GetAnimationTagNames())
		{
			_animations.Add(animation, _spriteSheet.CreateAnimatedSprite(animation));
		}
	}

	// should be a looping animation (LoopCount == 0)
	public void SetAnimation(string animation_name)
	{
		if (_currentAnimation == animation_name)
		{
			return;
		}

		if (!_animations.TryGetValue(animation_name, out var animation))
		{
			return;
		}

		if (animation.LoopCount != 0)
		{
			return;
		}

		_animations[_currentAnimation].Stop();

		_currentAnimation = animation_name;
		animation.Play(startingFrame: 0);
	}

	// runs a non-looping animation
	public void RunAnimation(string animation_name)
	{
		if (!_animations.TryGetValue(animation_name, out var animation))
		{
			return;
		}

		if (animation.LoopCount == 0)
		{
			return;
		}

		_animations[_currentAnimation].Pause();

		_overrideAnimation = animation_name;
		animation.Play(startingFrame: 0);
	}

	public string GetAnimation()
	{
		return _overrideAnimation != null ? _overrideAnimation : _currentAnimation;
	}

	public void Update(GameTime gameTime)
	{
		if (_overrideAnimation == null)
		{
			_animations[_currentAnimation].Update(gameTime);
			return;
		}

		_animations[_overrideAnimation].Update(gameTime);

		if (_animations[_overrideAnimation].IsAnimating == false)
		{
			_overrideAnimation = null;
		}
	}

	public TextureRegion GetFrame()
	{
		return _animations[_overrideAnimation != null ? _overrideAnimation : _currentAnimation].CurrentFrame.TextureRegion;
	}

	public Animation Clone()
	{
		return new Animation(_spriteSheet, _currentAnimation);
	}
}