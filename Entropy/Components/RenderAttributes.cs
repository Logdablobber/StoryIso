using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Entropy.Enums;
using Entropy.Misc;

namespace Entropy.Entities;

public class RenderAttributes
{
	public readonly string Name;
	public bool Visible { get; set; }
	public Color color { get; set; }
    public Color ColorWithOpacity
	{
		get
		{
			var opacity = GetOpacityMultiplier();

			return new Color(color.R, color.G, color.B, color.A * opacity);
		}
	}
	
	// this is whether the element is drawn to the screen (i.e. UI element)
	// or whether its drawn to the world (i.e. Character)
	public bool ScreenElement { get; set; }
	private Vector2? _origin { get; set; }
	public RenderLayer renderLayer { get; set; }
	public Dictionary<AnimationType, float> Animations = [];
	private Dictionary<AnimationType, float> _animationTimers = [];

	public RenderAttributes(string name, bool visible, Color color, RenderLayer layer, Vector2? origin = null)
	{
		this.Name = name;
		this.Visible = visible;
		this.color = color;
		this.ScreenElement = false;
		this._origin = origin;
		this.renderLayer = layer;
	}

	public RenderAttributes(string name, bool visible, Color color, bool screen_element, RenderLayer layer, Vector2? origin = null)
	{
		this.Name = name;
		this.Visible = visible;
		this.color = color;
		this.ScreenElement = screen_element;
		this._origin = origin;
		this.renderLayer = layer;
	}

	public Vector2 GetOrigin(Vector2 size)
	{
		if (!this._origin.HasValue)
		{
			return Vector2.Zero;
		}

		return size * this._origin.Value;
	}

	public void Update(float deltaTime)
	{
		foreach (var animation_type in _animationTimers.Keys)
		{
			var current_value = _animationTimers[animation_type] + deltaTime;
            
            _animationTimers[animation_type] = current_value % Animations[animation_type];
		}
	}

	public Dictionary<AnimationType, float> GetAnimations()
	{
		Dictionary<AnimationType, float> animations = [];
        
		foreach (var (animation, timer) in _animationTimers)
		{
			var max_value = Animations[animation];
            
            var interpolated_value = MiscFuncs.InverseLerp(0, max_value, timer);

			animations.Add(animation, interpolated_value);
		}

		return animations;
	}
    
    // the values passed in as floats should be between 0 and 1
    private float GetOpacityMultiplier()
    {
	    var animations = GetAnimations();
        
	    if (animations.Count == 0)
	    {
		    return 1;
	    }

	    if (animations.TryGetValue(AnimationType.Blinking, out var blinking_value) && blinking_value > 0.5f)
	    {
		    return 0;
	    }

	    if (animations.TryGetValue(AnimationType.Fading, out var fading_value))
	    {
		    // smoothly fades between 0 and 1 where both x = 0 and x = 1 have a value of 1
		    return MathF.Cos(MathF.Tau * fading_value) * 0.5f + 0.5f;
	    }

	    return 1;
    }
    
    public void StartAnimation(AnimationType type, float parameter)
    {
	    Animations[type] = parameter;
	    _animationTimers[type] = 0f;
    }

	public void ResetAnimation(AnimationType type)
	{
		if (!Animations.ContainsKey(type))
		{
			return;
		}

		_animationTimers[type] = 0f;
	}
    
    public void StopAnimation(AnimationType type)
    {
	    Animations.Remove(type);
	    _animationTimers.Remove(type);
    }
    
    public void StopAllAnimations()
	{
		Animations.Clear();
        _animationTimers.Clear();
	}
}