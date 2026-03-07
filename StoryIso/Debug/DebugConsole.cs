using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using StoryIso.Misc;

namespace StoryIso.Debugging;

public static class DebugConsole
{
	public static BitmapFont? Font;
	public static float Scale;

	private static readonly List<DebugLine> _lines = [];

	private static readonly System.Threading.Lock _lineLock = new();

	const float LINE_PERSISTENCE = 10f;
	const float LINE_SPACING = 40f;
	const float WRAP_SPACING = 38f;
	const float YMARGIN = 20f;
	const float XMARGIN = 20f;

	public static void WriteLine(string text, Color color, bool stack = false)
	{
		lock (_lineLock)
		{
			if (!stack || _lines.Count == 0 || !_lines[^1].CheckIncrementCount(text, color))
			{
				_lines.Add(new DebugLine(text, color));
			}
		}
		Debug.WriteLine(text);
	}

	public static void Raise(IError error)
	{
		WriteLine(error.GetMessage(), Color.Red, stack: true);
	}

	public static void Update(GameTime gameTime)
	{
		for (int i = 0; i < _lines.Count; i++)
		{
			if (!_lines[i].visible)
			{
				continue;
			}

			if ((DateTime.Now - _lines[i].creationTime).TotalSeconds >= LINE_PERSISTENCE)
			{
				_lines[i].visible = false;
			}
		}
	}

	public static void Draw(SpriteBatch spriteBatch)
	{
		if (Font == null)
		{
			throw new NullReferenceException("No font :(");
		}

		float screen_width = Game1.camera.BoundingRectangle.Width - 2 * XMARGIN;

		int index = 0;
		int wrap_index = 0;
		lock (_lineLock)
		{
			foreach (var line in _lines)
			{
				if (!line.TryGet(out string? text) || text == null)
				{
					continue;
				}

				List<string> wrapped_lines = TextFormatter.WrapText(text, Font, Scale, screen_width);

				for (int i = 0; i < wrapped_lines.Count; i++)
				{
					var position = new Vector2(XMARGIN, YMARGIN + index * LINE_SPACING + (wrap_index + i) * WRAP_SPACING) * Scale + Game1.cameraOffset;

					spriteBatch.DrawString(Font, wrapped_lines[i], position, line.color, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
				}

				index++;
				wrap_index += wrapped_lines.Count - 1;
			}
		}
	}
}