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
using System.Linq;
using Microsoft.Toolkit.HighPerformance.Buffers;

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
	public bool WrapText { get; set; }

	private static readonly Regex _formatRegex = FormatRegex();

	public TextComponent(string name, string text, string fontName, float fontSize, SizeF size, TextAlignment alignment, bool wrap_text)
	{
		this.name = name;
		SetText(text);
		this.FontName = fontName;
		this.FontSize = fontSize;
		this.Size = size;
		this.Alignment = alignment;
		this.WrapText = wrap_text;
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

	public void Draw(SpriteBatch spriteBatch, Color color, Vector2 position, Vector2 scale, float rotation, float layer_depth)
	{
		FontInstance? font = FontLoader.GetFont(this.FontName);

		if (font == null)
		{
			return;
		}

		string fitted_text = TextFormatter.FitText(this.Text, font, this.FontSize, this.Size, out float scale_mult, wrap_text:this.WrapText).JoinToString('\n');

		Vector2 draw_scale = scale * scale_mult;

		RectangleF text_rect = font.Font.GetStringRectangle(fitted_text);

		float y_origin;
		float y_position;
		switch (Alignment.VerticalAlignment)
		{
			case VerticalTextAlignment.Top: 
				y_origin = 0;
				y_position = position.Y;
				break;

			case VerticalTextAlignment.Center:
				y_origin = text_rect.Center.Y;
				y_position = position.Y + Size.Height * scale.Y * 0.5f;
				break;

			case VerticalTextAlignment.Bottom:
				y_origin = text_rect.Bottom;
				y_position = position.Y + Size.Height * scale.Y;
				break;

			default:
				throw new NotImplementedException();
		}

		float x_origin;
		float x_position;
		switch (Alignment.HorizontalAlignment)
		{
			case HorizontalTextAlignment.Left: 
				x_origin = 0;
				x_position = position.X;
				break;

			case HorizontalTextAlignment.Center:
				x_origin = text_rect.Center.X;
				x_position = position.X + Size.Width * scale.X * 0.5f;
				break;

			case HorizontalTextAlignment.Right:
				x_origin = text_rect.Right;
				x_position = position.X + Size.Width * scale.X;
				break;

			default:
				throw new NotImplementedException();
		}

		Vector2 origin = new Vector2(x_origin, y_origin);
		
		Vector2 draw_position = new Vector2(x_position, y_position);

		// draw hitboxes of text boxes
		//spriteBatch.DrawRectangle(new RectangleF(position, Size * scale), new Color(1f, 0, 1f, 0.5f));
		//spriteBatch.DrawLine(new Vector2(position.X, position.Y + Size.Height * 0.5f * scale.Y), new Vector2(position.X + Size.Width * scale.X, position.Y + Size.Height * 0.5f * scale.Y), new Color(1f, 0, 1f, 0.5f));

		spriteBatch.DrawString(font.Font, fitted_text, draw_position, color, 0, origin, draw_scale, SpriteEffects.None, layer_depth);
	}

	[GeneratedRegex(@"(?<={)[^}]+(?=})", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.RightToLeft, "en-US")]
	private static partial Regex FormatRegex();
}