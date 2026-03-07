using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using StoryIso.Debugging;
using StoryIso.Misc;

namespace StoryIso.Dialogue;

public class DialogueManager
{
	public Texture2D DialogueBox;
	public Texture2D NameBox;
	public float Scale;
	private Vector2 _screenPosition
	{
		get
		{
			return Position + Game1.cameraOffset;
		}
	}
	public Vector2 Position;
	
	public BitmapFont Font;
	public float FontScale;

	private readonly Dictionary<string, DialogueSequence> dialogues;
	private string? currentDialogueName;
	const float NAME_SCALE = 0.8f;
	private DialogueSequence? currentDialogueSequence
	{
		get
		{
			if (currentDialogueName == null)
			{
				return null;
			}

			return dialogues[currentDialogueName];
		}
	}
	private DialogueStep? currentDialogueStep
	{
		get
		{
			if (currentDialogueSequence == null || !dialogueIndex.HasValue || dialogueIndex.Value >= currentDialogueSequence.dialogueSteps.Count)
			{
				return null;
			}

			return currentDialogueSequence.dialogueSteps[dialogueIndex.Value];
		}
	}
	private float? currentDialogueDuration
	{
		get
		{
			if (!currentDialogueStep.HasValue)
			{
				return null;
			}

			return currentDialogueStep.Value.duration ?? TIMEPERCHARACTER * currentDialogueStep.Value.text.Length;
		}
	}

	private int? dialogueIndex;
	private bool _continueKeyPressedLastFrame = false;
	private bool _skipKeyPressedLastFrame = false;
	private float? _dialogueTimer;
	const float TIMEPERCHARACTER = 0.05f;

	public bool Active
	{
		get
		{
			return currentDialogueStep.HasValue && dialogueIndex.HasValue && _dialogueTimer.HasValue;
		}
	}

	private const float LEFTMARGIN = 6.6f;
	private const float RIGHTMARGIN = 6.6f;
	private const float TOPMARGIN = 6.6f;
	private const float BOTTOMMARGIN = 6.6f;

	private SizeF unscaledTextBounds
	{
		get
		{
			return DialogueBox.Bounds.Size.ToVector2() - new SizeF(LEFTMARGIN + RIGHTMARGIN, TOPMARGIN + BOTTOMMARGIN);
		}
	}
	private SizeF dialogueTextBounds
	{
		get
		{
			return unscaledTextBounds * Scale;
		}
	}

	public DialogueManager(Texture2D dialogue_box_texture,
							Texture2D name_box_texture,
							float scale,
							Vector2 position,
							BitmapFont font,
							float font_scale,
							string dialogue_directory)
	{
		dialogues = new Dictionary<string, DialogueSequence>();

		DialogueBox = dialogue_box_texture;
		NameBox = name_box_texture;
		Scale = scale;
		Position = position;
		Font = font;
		FontScale = font_scale;
		LoadDialogues(dialogue_directory);
	}

	private void LoadDialogues(string directory)
	{
		var dir = new DirectoryInfo(directory);

		var files = dir.GetFiles("*.json");

		foreach (var file in files)
		{
			if (file.Name == "DialogueSchema.json")
			{
				continue;
			}

			LoadDialogue(file.FullName);
		}

		#if DEBUG

		JsonNode schema = Game1.DeserializeOptions.GetJsonSchemaAsNode(typeof(DialogueSequence));

		var path = Path.GetFullPath("./");

		var src_path = Regex.Replace(path, @"bin.+", string.Empty);

		using (var f = new StreamWriter(Path.Combine(src_path, directory, "DialogueSchema.json")))
		{
			f.Write(schema.ToJsonString(Game1.DeserializeOptions));
		}

		#endif
	}

	private void LoadDialogue(string path)
	{
		string json;
		using (StreamReader streamReader = new StreamReader(path, Encoding.UTF8))
		{
			json = streamReader.ReadToEnd();
		}

		DialogueSequence? sequence = DialogueGenerator.Generate(json);

		if (sequence == null)
		{
			return;
		}

		dialogues.Add(sequence.id, sequence);
	}

	public void RunDialogue(string name, Source source)
	{
		if (dialogues.ContainsKey(name))
		{
			currentDialogueName = name;
			dialogueIndex = 0;
			_dialogueTimer = 0f;
		}
		else
		{
			DebugConsole.Raise(new UnknownDialogueError(source, "RunDialogue", name));
		}
 	}

	public void EndDialogue(Source? source = null)
	{
		if (!Active)
		{
			DebugConsole.Raise(new DialogueNotRunningError(source));
			return;
		}

		currentDialogueName = null;
		dialogueIndex = null;
		_dialogueTimer = null;
	}

	public void Update(GameTime gameTime)
	{
		if (!Active)
		{
			return;
		}

		var keystate = Keyboard.GetState();

		_dialogueTimer += (float)gameTime.ElapsedGameTime.TotalSeconds * (currentDialogueStep!.Value.speedMultiplier ?? 1f);

		if (keystate.IsKeyDown(Keys.X) || keystate.IsKeyDown(Keys.RightShift) || keystate.IsKeyDown(Keys.LeftShift)) 
		{
			if (!currentDialogueStep.Value.preventSkip && !_skipKeyPressedLastFrame)
			{
				_dialogueTimer = currentDialogueDuration;
			}

			_skipKeyPressedLastFrame = true;
		}
		else
		{
			_skipKeyPressedLastFrame = false;
		}

		if (keystate.IsKeyDown(Keys.Z) || keystate.IsKeyDown(Keys.Enter))
		{
			if (!_continueKeyPressedLastFrame && _dialogueTimer >= currentDialogueDuration &&
				dialogueIndex < currentDialogueSequence!.dialogueSteps.Count)
			{	
				dialogueIndex += 1;
				_dialogueTimer = 0f;
			}

			_continueKeyPressedLastFrame = true;
		}
		else
		{
			_continueKeyPressedLastFrame = false;
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		if (!Active)
		{
			return;
		}

		spriteBatch.Draw(DialogueBox, _screenPosition, null, Color.White, 0, new Vector2(0, DialogueBox.Height), Scale, SpriteEffects.None, 0);

		DialogueStep current_step = currentDialogueStep!.Value;

		if (current_step.speaker != "")
		{
			DrawNameBox(spriteBatch, current_step.speaker);
		}

		string dialogue_text = current_step.text[..(int)Math.Min(Math.Ceiling(_dialogueTimer!.Value / TIMEPERCHARACTER), current_step.text.Length)];

		List<string> fitted_text = TextFormatter.FitText(dialogue_text, Font, FontScale, unscaledTextBounds, out float scale_mult);

		Vector2 top_left = _screenPosition + (new Vector2(LEFTMARGIN, TOPMARGIN) - new Vector2(0, DialogueBox.Height)) * Scale;
		float text_scale = FontScale * scale_mult * Scale;

		var color = current_step.color ?? Color.Black;

		for (int i = 0; i < fitted_text.Count; i++)
		{
			Vector2 position = top_left + new Vector2(0, i * Font.LineHeight * text_scale);

			spriteBatch.DrawString(Font, fitted_text[i], position, color, 0, Vector2.Zero, text_scale, SpriteEffects.None, 0f);
		}
	}

	private void DrawNameBox(SpriteBatch spriteBatch, string name)
	{
		const int Y_PIXEL_OFFSET = 7;

		Vector2 name_box_position = _screenPosition - new Vector2(0, DialogueBox.Height * Scale);

		spriteBatch.Draw(NameBox, name_box_position, null, Color.White, 0, new Vector2(0, NameBox.Height), Scale, SpriteEffects.None, 0f);

		Vector2 text_origin = Font.GetStringRectangle(name).Center - new Vector2(0, Y_PIXEL_OFFSET);

		spriteBatch.DrawString(Font, name, name_box_position + new Vector2(NameBox.Width / 2, -NameBox.Height / 2) * Scale, Color.Black, 0, text_origin, Scale * FontScale * NAME_SCALE, SpriteEffects.None, 0f);
	}
}