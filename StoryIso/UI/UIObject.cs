using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using StoryIso.Debugging;
using StoryIso.Entities;
using StoryIso.FileLoading;
using System.Linq;

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
	
	public UIObject(UIData data, World world)
	{
		this.Parts = new List<Entity>();

		this.info = new UIInfo(null, data.Position, new Vector2(data.Scale ?? 1f), data.Visible ?? true, data.Name);

		foreach (var part in data.Parts)
		{
			var entity = world.CreateEntity();

			Color color;

			if (part.Content is TextContent textContent)
			{
				var size = textContent.Size.ToSize();

				color = textContent.color ?? Color.Black;

				entity.Attach(new TextComponent(part.Name, textContent.text, textContent.font, size));
			}

			else if (part.Content is ImageContent imageContent)
			{
				var texture = TextureLoader.GetTexture(imageContent.image);

				if (texture == null)
				{
					DebugConsole.Raise(new MissingAssetError(new Source(0, null, $"Object {info.Name}"), imageContent.image, "Texture doesn't exist"));
					return;
				}

				color = imageContent.color ?? Color.White;

				entity.Attach(texture);
			}

			else
			{
				return;
			}

			entity.Attach(new Transform2());
			entity.Attach(new RenderAttributes(data.Visible ?? true, color, true));
			entity.Attach(new UIInfo(info, part.Position, new Vector2(part.Scale ?? 1f), part.Visible ?? true, part.Name));

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