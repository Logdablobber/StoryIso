using System.Collections.Generic;
using System.Data.SqlTypes;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using MonoGame.Extended.ECS;
using StoryIso.Debugging;
using StoryIso.Entities;
using StoryIso.FileLoading;

namespace StoryIso.UI;

public class UIObject
{
	public string Name { get; private set; }
	public Vector2 Position { get; set; }

	private List<Entity> Parts { get; set; }
	
	public UIObject(UIData data, World world)
	{
		this.Name = data.Name;
		this.Position = data.Position;

		this.Parts = new List<Entity>();

		foreach (var part in data.Parts)
		{
			var entity = world.CreateEntity();

			Color color = Color.White;

			if (part.Content is TextContent textContent)
			{
				var size = textContent.Size.ToSize();

				color = textContent.color ?? Color.Black;

				entity.Attach(new TextComponent(textContent.text, textContent.font, size));
			}

			else if (part.Content is ImageContent imageContent)
			{
				var texture = TextureLoader.GetTexture(imageContent.image);

				if (texture == null)
				{
					DebugConsole.Raise(new MissingAssetError(new Source(0, null, $"Object {Name}"), imageContent.image, "Texture doesn't exist"));
					return;
				}

				color = imageContent.color ?? Color.White;

				entity.Attach(texture);
			}

			else
			{
				return;
			}

			Vector2 position = part.Position + Position;

			entity.Attach(new Transform2(position: position, scale: new Vector2(part.Scale ?? 1)));
			entity.Attach(new RenderAttributes(data.Visible ?? true, color, true));

			this.Parts.Add(entity);
		}
	}

	public void UpdatePosition(Vector2 new_position)
	{
		Vector2 delta_position = Position - new_position;

		foreach (var part in Parts)
		{
			var transform = part.Get<Transform2>();

			transform.Position += delta_position;
		}
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