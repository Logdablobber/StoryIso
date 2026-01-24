using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended.Screens;
using StoryIso.Misc;

namespace StoryIso.Debugging;

public static class DebugConsole
{
	public static BitmapFont Font;
	public static float Scale;

	private static List<DebugLine> _lines = new List<DebugLine>();

	private static float _timer = 0f;
	const float LINEPERSISTANCE = 10f; // how long a line is on the screen before fading away
	const float LINESPACING = 40f;
	const float WRAPSPACING = 38f;
	const float YMARGIN = 20f;
	const float XMARGIN = 20f;

	public static void WriteLine(string text, Color color)
	{
		_lines.Add(new DebugLine(_timer, text, color));
		Debug.WriteLine(text);
	}

	public static void Raise(IError error)
	{
		WriteLine(error.GetMessage(), Color.Red);
	}

	public static void Update(GameTime gameTime)
	{
		_timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

		for (int i = 0; i < _lines.Count; i++)
		{
			if (!_lines[i].visible)
			{
				continue;
			}

			if (_timer - _lines[i].creationTime >= LINEPERSISTANCE)
			{
				_lines[i].visible = false;
			}
		}
	}

	public static void Draw(SpriteBatch spriteBatch)
	{
		float screen_width = Game1.camera.BoundingRectangle.Width - 2 * XMARGIN;

		int index = 0;
		int wrap_index = 0;
		foreach (var line in _lines)
		{
			if (!line.visible)
			{
				continue;
			}

			List<string> wrapped_lines = TextFormatter.WrapText(line.text, Font, Scale, screen_width);

			for (int i = 0; i < wrapped_lines.Count; i++)
			{
				var position = new Vector2(XMARGIN, YMARGIN + index * LINESPACING + (wrap_index + i) * WRAPSPACING) * Scale + Game1.cameraOffset;

				spriteBatch.DrawString(Font, wrapped_lines[i], position, line.color, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
			}

			index++;
			wrap_index += wrapped_lines.Count - 1;
		}
	}
}