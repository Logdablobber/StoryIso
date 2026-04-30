using Microsoft.Xna.Framework;

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

	private string _name;
	public string Name
	{
		get
		{
			if (this.Parent != null)
			{
				return $"{Parent.Name}.{_name}";
			}

			return _name;
		}
        set
        {
	        _name = value;
        }
	}

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
		this._name = name;
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