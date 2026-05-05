using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using Entropy.Debugging;
using Entropy.Entities;
using Entropy.FileLoading;
using System.Linq;
using MonoGame.Extended.Shapes;
using Entropy.ECS;
using Microsoft.Xna.Framework.Graphics;
using Entropy.Enums;
using Entropy.Misc;

namespace Entropy.UI;

public class UIObject
{
	public UIInfo info { get; private set; }
    private Entity _objEntity { get; set; }

	private Entity[] _parts { get; set; }
	private UIObject[] _children { get; set; }

	public string[] Parts
	{
		get
		{
			return (from part in _parts select part.Get<UIInfo>().Name).ToArray();
		}
	}

	public UIObject[] Children
	{
		get
		{
			return _children;
		}
	}
	
	public UIObject(UIObject? parent, GraphicsDevice graphics, UIData data, World world)
	{
		this._children = new UIObject[data.Children?.Length ?? 0];
		this._parts = new Entity[data.Parts?.Length ?? 0];

		this.info = new UIInfo(parent?.info, data.Position.ToVector2(), new Vector2(data.Scale ?? 1f), data.Visible ?? true, data.Name);

		this._objEntity = world.CreateEntity();
        this._objEntity.Attach(this.info);

		if (data.Children != null)
		{
			for (int i = 0; i < data.Children.Length; i++)
			{
				_children[i] = new UIObject(this, graphics, data.Children[i], world);
			}
		}

		if (data.Parts == null)
		{
			return;
		}

		for (int i = 0; i < data.Parts.Length; i++)
		{
			var part = data.Parts[i];

			var entity = world.CreateEntity();

			Color? color = null;

			switch (part.Content)
			{
				case TextContent textContent:

					var size = textContent.Size.ToSizeF();

					color = textContent.color ?? Color.Black;

					var alignment = new TextAlignment
					{
						VerticalAlignment = textContent.vAlignment ?? VerticalTextAlignment.Top,
						HorizontalAlignment = textContent.hAlignment ?? HorizontalTextAlignment.Left
					};

					entity.Attach(new TextComponent(part.Name, textContent.text, textContent.font, textContent.fontSize, size, alignment, textContent.WrapText ?? true));
					break;

				case ImageContent imageContent:
		
					var texture = TextureLoader.GetTexture(imageContent.image);

					if (texture == null)
					{
						DebugConsole.Raise(new MissingAssetError(new Source(0, null, $"Object {info.Name}"), imageContent.image, "Texture doesn't exist"));
						return;
					}

					color = imageContent.color ?? Color.White;

					entity.Attach(texture);
					break;

				case ButtonContent buttonContent:
					var button_size = buttonContent.Size.ToSizeF();

					entity.Attach(new ButtonComponent(part.Name, 
													button_size, 
													buttonContent.OnLeftClick,
													buttonContent.OnLeftRelease,
													buttonContent.WhileLeftHeld,
													buttonContent.OnRightClick,
													buttonContent.OnRightRelease,
													buttonContent.WhileRightHeld,
													buttonContent.OnEnter,
													buttonContent.OnExit,
													buttonContent.OnStay));
					break;

				case RectangleContent rectangleContent:
					color = rectangleContent.color;

					entity.Attach(new RectangleComponent(rectangleContent.Size.ToSizeF(), rectangleContent.OutlineWidth, rectangleContent.OutlineColor));
					break;

				case PolygonContent polygonContent:
					color = polygonContent.color;

					Vector2[] vertices = (from vert in polygonContent.Vertices select vert.ToVector2()).ToArray();

					entity.Attach(new PolygonComponent(graphics, vertices));
					break;

				default:
					break;
			}

			entity.Attach(new RenderAttributes($"{info.Name}.{part.Name}", data.Visible ?? true, color ?? Color.White, true, RenderLayer.UI, part.Origin?.ToVector2()));
			entity.Attach(new UIInfo(info, part.Position.ToVector2(), new Vector2(part.Scale ?? 1f), part.Visible ?? true, part.Name));

			this._parts[i] = entity;
		}
	}

	public void UpdatePosition(Vector2 new_position)
	{
		info.LocalPosition = new_position;
	}

	public void UpdateX(float new_x)
	{
		info.SetX(new_x);
	}

	public void UpdateY(float new_y)
	{
		info.SetY(new_y);
	}
	
	public void Destroy()
	{
		for (int i = this._parts.Length - 1; i >= 0; i--)
		{
			this._parts[i].Destroy();
		}

		this._parts = [];

		for (int i = this._children.Length - 1; i >= 0; i--)
		{
			this._children[i].Destroy();
		}

		this._children = [];
	}
}