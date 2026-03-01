using System.Diagnostics.Tracing;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace StoryIso.Entities;

public class UIInfo
{
	public UIInfo? Parent;
	private Vector2 _position;
	private bool _visible;
	public bool Visible
	{
		get
		{
			if (Parent == null)
			{
				return _visible;
			}

			return _visible && Parent.Visible;
		}
		set
		{
			_visible = value;
		}
	}

	public string Name;

	public Vector2 LocalPosition
	{
		get
		{
			return _position;
		}
		set
		{
			_position = value;
		}
	}

	public Vector2 Position
	{
		get
		{
			if (Parent == null)
			{
				return _position;
			}

			return _position * Parent.Scale + Parent.Position;
		}
	}

	private Vector2 _scale;
	public Vector2 Scale
	{
		get
		{
			if (Parent == null)
			{
				return _scale;
			}

			return Parent.Scale * _scale;
		}
		set
		{
			_scale = value;
		}
	}

	public UIInfo(UIInfo? parent, Vector2 position, Vector2 scale, bool visible, string name)
	{
		this.Parent = parent;
		this._position = position;
		this._scale = scale;
		this.Visible = visible;
		this.Name = name;
	}

	public void SetX(float x)
	{
		_position.X = x;
	}

	public void SetY(float y)
	{
		_position.Y = y;
	}
}