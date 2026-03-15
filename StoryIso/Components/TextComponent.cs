using System;
using System.Text.RegularExpressions;
using MonoGame.Extended;
using StoryIso.Debugging;
using StoryIso.Scripting;

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
				ParameterProcessor.ConvertUnknown(_params[i], out var string_value);

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
	public SizeF Size { get; set; }

	private static readonly Regex _formatRegex = FormatRegex();

	public TextComponent(string name, string text, string fontName, SizeF size)
	{
		this.name = name;
		SetText(text);
		this.FontName = fontName;
		this.Size = size;
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

			FunctionParameter<string>? param = ParameterProcessor.ParseEquation<string>(matches[i].Value, new Source(0, null, $"TextComponent {name}"), "TextComponent.Parse", null);

			if (param == null)
			{
				_params[i] = new FunctionParameter<string>(string.Empty);
				continue;
			}

			_params[i] = param.Value;
		}

		_text = text;
	}

	[GeneratedRegex(@"(?<={)[^}]+(?=})", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.RightToLeft, "en-US")]
	private static partial Regex FormatRegex();
}