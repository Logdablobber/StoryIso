using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using StoryIso.Debugging;
using StoryIso.Entities;
using StoryIso.FileLoading;
using System.Linq;
using MonoGame.Extended.Shapes;
using StoryIso.ECS;
using Microsoft.Xna.Framework.Graphics;
using StoryIso.Enums;
using StoryIso.Misc;

namespace StoryIso.UI;

public class UIObject
{
	public UIInfo info { get; private set; }

	private List<Entity> Parts { get; set; }

	public string[] Children
	{
		get
		{
			return (from part in Parts select part.Get<UIInfo>().Name).ToArray();
		}
	}
	
	public UIObject(GraphicsDevice graphics, UIData data, World world)
	{
		this.Parts = new List<Entity>();

		this.info = new UIInfo(null, data.Position.ToVector2(), new Vector2(data.Scale ?? 1f), data.Visible ?? true, data.Name);

		foreach (var part in data.Parts)
		{
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

					entity.Attach(new TextComponent(part.Name, textContent.text, textContent.font, textContent.fontSize, size, alignment));
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

			entity.Attach(new Transform2());
			if (color.HasValue)
			{
				entity.Attach(new RenderAttributes(data.Visible ?? true, color.Value, true));
			}
			entity.Attach(new UIInfo(info, part.Position.ToVector2(), new Vector2(part.Scale ?? 1f), part.Visible ?? true, part.Name));

			this.Parts.Add(entity);
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
		for (int i = this.Parts.Count - 1; i >= 0; i--)
		{
			this.Parts[i].Destroy();
		}

		this.Parts.Clear();
	}
}