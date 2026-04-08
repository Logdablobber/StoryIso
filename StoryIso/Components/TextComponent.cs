using System;
using System.Text.RegularExpressions;
using MonoGame.Extended;
using StoryIso.Debugging;
using StoryIso.Scripting;
using StoryIso.Enums;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGame.Extended.BitmapFonts;
using StoryIso.FileLoading;
using StoryIso.Misc;
using System.Collections.Generic;
using System.Diagnostics;

namespace StoryIso.Entities;

public partial class TextComponent
{
	private readonly string name;
	private string _text = null!;
	private FunctionParameter<string>[] _params = null!;
	public string Text
	{
		get
		{
			if (_params.Length == 0)
			{
				return _text;
			}

			string[] _formatParams = new string[_params.Length];

			for (int i = 0; i < _params.Length; i++)
			{
				ParameterProcessor.ConvertUnknown(new Source(0, "TextComponent.GetText", $"TextComponent {name}"), _params[i], out var string_value);

				if (string_value == null)
				{
					_formatParams[i] = string.Empty;
					continue;
				}

				_formatParams[i] = string_value;
			}

			return string.Format(_text, _formatParams);
		}
	}


	public string FontName { get; set; }
	public float FontSize { get; set; }
	public SizeF Size { get; set; }
	public TextAlignment Alignment { get; set; }

	private static readonly Regex _formatRegex = FormatRegex();

	public TextComponent(string name, string text, string fontName, float fontSize, SizeF size, TextAlignment alignment)
	{
		this.name = name;
		SetText(text);
		this.FontName = fontName;
		this.FontSize = fontSize;
		this.Size = size;
		this.Alignment = alignment;
	}

	public void SetText(string text)
	{
		var matches = _formatRegex.Matches(text);

		if (matches.Count == 0)
		{
			this._text = text;
			this._params = [];
			return;
		}

		this._params = new FunctionParameter<string>[matches.Count];

		for (int i = 0; i < matches.Count; i++)
		{
			text = text[..matches[i].Index] + i.ToString() + text[Math.Min(text.Length - 1, matches[i].Index + matches[i].Length + 2)..];

			FunctionParameter<string>? param = ParameterProcessor.ParseEquation<string>(Game1.GlobalScope, matches[i].Value, new Source(0, null, $"TextComponent {name}"), "TextComponent.Parse");

			if (param == null)
			{
				_params[i] = new FunctionParameter<string>(string.Empty);
				continue;
			}

			_params[i] = param.Value;
		}

		_text = text;
	}

	public void Draw(SpriteBatch spriteBatch, Color color, Vector2 position, Vector2 scale, float rotation)
	{
		// TODO: Make this wrap
		FontInstance? font = FontLoader.GetFont(this.FontName);

		if (font == null)
		{
			return;
		}

		string[] fitted_text = TextFormatter.FitText(this.Text, font, this.FontSize, this.Size, out float scale_mult);

		Vector2 draw_scale = scale * scale_mult;

		if (Alignment.VerticalAlignment == VerticalTextAlignment.Top)
		{
			for (int i = 0; i < fitted_text.Length; i++)
			{
				float y_position = position.Y + (font.Font.LineSpacing + font.Font.LineHeight) * draw_scale.Y * i;

				DrawLine(spriteBatch, font.Font, fitted_text[i], position.X, position.X + Size.Width * draw_scale.X, y_position, color, rotation, scale);
			}
			
			return;
		}

		if (Alignment.VerticalAlignment == VerticalTextAlignment.Bottom)
		{
			for (int i = 0; i < fitted_text.Length; i++)
			{
				float y_position = position.Y + (Size.Height - (font.Font.LineSpacing + font.Font.LineHeight) * i) * draw_scale.Y;

				DrawLine(spriteBatch, font.Font, fitted_text[i], position.X, position.X + Size.Width * draw_scale.X, y_position, color, rotation, scale);
			}

			return;
		}

		// TODO: fix this
		if (Alignment.VerticalAlignment != VerticalTextAlignment.Center)
		{
			throw new NotImplementedException();
		}

		for (int i = 0; i < fitted_text.Length; i++)
		{
			float y_position = position.Y + (Size.Height / 2 + (font.Font.LineSpacing + font.Font.LineHeight) * (i - fitted_text.Length / 2.0f)) * draw_scale.Y;

			DrawLine(spriteBatch, font.Font, fitted_text[i], position.X, position.X + Size.Width * draw_scale.X, y_position, color, rotation, scale);
		}
	}

	private void DrawLine(SpriteBatch spriteBatch, BitmapFont font, string text, float min_x, float max_x, float y_position, Color color, float rotation, Vector2 scale)
	{
		if (Alignment.HorizontalAlignment == HorizontalTextAlignment.None)
		{
			throw new UnreachableException();
		}

		SizeF text_size = font.MeasureString(text);
		float y_origin = Alignment.VerticalAlignment == VerticalTextAlignment.Bottom ? text_size.Height : 0;

		if (Alignment.HorizontalAlignment == HorizontalTextAlignment.Left)
		{
			Vector2 line_position = new(min_x, y_position);

			spriteBatch.DrawString(font, text, line_position, color, rotation, new Vector2(0, y_origin), scale, SpriteEffects.None, 0f);
			return;
		}

		if (Alignment.HorizontalAlignment == HorizontalTextAlignment.Right)
		{
			Vector2 line_position = new(max_x, y_position);

			spriteBatch.DrawString(font, text, line_position, color, rotation, new Vector2(text_size.Width, y_origin), scale, SpriteEffects.None, 0f);
			return;
		}

		if (Alignment.HorizontalAlignment == HorizontalTextAlignment.Center)
		{
			Vector2 line_position = new((min_x + max_x) / 2, y_position);

			spriteBatch.DrawString(font, text, line_position, color, rotation, new Vector2(text_size.Width / 2, y_origin), scale, SpriteEffects.None, 0f);
			return;
		}

		throw new NotImplementedException();
	}

	[GeneratedRegex(@"(?<={)[^}]+(?=})", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.RightToLeft, "en-US")]
	private static partial Regex FormatRegex();
}