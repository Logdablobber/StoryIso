using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.RegularExpressions;
using Microsoft.Toolkit.HighPerformance.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using SharpDX.Direct3D;
using Entropy.Debugging;
using Entropy.ECS;
using Entropy.Misc;
using Entropy.UI;

namespace Entropy.Dialogue;

public class DialogueManager
{
	public Texture2D DialogueBox;
	public Texture2D NameBox;
	public Texture2D OptionBox;
	public float Scale;
	private Vector2 _screenPosition
	{
		get
		{
			return Position + Game1.CameraOffset;
		}
	}
	public Vector2 Position;
	
	public FontInstance Font;
	public float FontScale;

	private readonly Dictionary<string, DialogueTree> dialogueTrees = [];
	private string? currentDialogueTreeName;
	private DialogueTree? currentDialogueTree
	{
		get
		{
			if (currentDialogueTreeName == null)
			{
				return null;
			}

			return dialogueTrees[currentDialogueTreeName];
		}
	}
	private DialogueNode? currentDialogueNode
	{
		get
		{
			if (currentDialogueTree == null)
			{
				return null;
			}

			return currentDialogueTree.currentNode;
		}
	}

	const float NAME_SCALE = 0.8f;
	private float? currentDialogueDuration
	{
		get
		{
			if (currentDialogueNode == null)
			{
				return null;
			}

			return currentDialogueNode.Duration ?? TIME_PER_CHARACTER * currentDialogueNode.text.Length;
		}
	}
	private float? _dialogueTimer;
	const float TIME_PER_CHARACTER = 0.05f;
	private int _currentCharacterIndex = 0;

	private KeyboardState? _previousKeyState = null;

	public bool Active
	{
		get
		{
			return currentDialogueNode != null && _dialogueTimer.HasValue;
		}
	}

	private const float LEFT_MARGIN = 6.6f;
	private const float RIGHT_MARGIN = 6.6f;
	private const float TOP_MARGIN = 6.6f;
	private const float BOTTOM_MARGIN = 6.6f;

	private SizeF unscaledTextBounds
	{
		get
		{
			return DialogueBox.Bounds.Size.ToVector2() - new SizeF(LEFT_MARGIN + RIGHT_MARGIN, TOP_MARGIN + BOTTOM_MARGIN);
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
							Texture2D option_box_texture,
							float scale,
							Vector2 position,
							FontInstance font,
							float font_scale,
							string dialogue_directory)
	{
		dialogueTrees = new Dictionary<string, DialogueTree>();

		DialogueBox = dialogue_box_texture;
		NameBox = name_box_texture;
		OptionBox = option_box_texture;
		Scale = scale;
		Position = position;
		Font = font;
		FontScale = font_scale;
		LoadDialogues(dialogue_directory);
	}

	private void LoadDialogues(string directory)
	{
		#if DEBUG

		JsonNode schema = Game1.DeserializeOptions.GetJsonSchemaAsNode(typeof(SerializableDialogueTree));

		var path = Path.GetFullPath("./");

		var src_path = Regex.Replace(path, @"bin.+", string.Empty);

		using (var f = new StreamWriter(Path.Combine(src_path, directory, "DialogueSchema.json")))
		{
			f.Write(schema.ToJsonString(Game1.DeserializeOptions));
		}

		#endif

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
	}

	private void LoadDialogue(string path)
	{
		string json;
		using (StreamReader streamReader = new StreamReader(path, Encoding.UTF8))
		{
			json = streamReader.ReadToEnd();
		}

		var tree = DialogueGenerator.Generate(json);

		if (tree == null)
		{
			return;
		}

		var unserialized_tree = tree.ToDialogueTree();

		dialogueTrees.Add(unserialized_tree.id, unserialized_tree);
	}

	public void SelectDialogueOption(int index, Source source)
	{
		if (currentDialogueNode == null)
		{
			DebugConsole.Raise(new DialogueNotRunningError(source));
			return;
		}

		currentDialogueTree!.Next(index, source);

		UpdateDialogue();
		_dialogueTimer = 0;
		_currentCharacterIndex = 0;
	}

	public void RunDialogue(string name, Source source)
	{
		if (dialogueTrees.ContainsKey(name))
		{
			currentDialogueTreeName = name;
			currentDialogueTree!.Reset();
			_dialogueTimer = 0f;
			_currentCharacterIndex = 0;

			UpdateDialogue();
			
			UIManager.SetObjectVisible("Dialogue", true);
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

		UIManager.SetObjectVisible("Dialogue", false);

		currentDialogueTreeName = null;
		_dialogueTimer = null;
	}

	private void UpdateDialogue()
	{
		if (currentDialogueNode == null)
		{
			throw new NullReferenceException();
		}

		if (currentDialogueNode.speaker != null)
		{	
			UISystem.SetAttributeChange("Dialogue.SpeakerBox.text", "text", new Optional<string>(currentDialogueNode.speaker));
			SetSpeakerBoxVisibility(true);
		}
		else
		{
			SetSpeakerBoxVisibility(false);
		}

		if (currentDialogueNode.Options != null && currentDialogueNode.Options.Length != 0)
		{
			if (currentDialogueNode.Options.Length > 3)
			{
				// TODO: raise error
			}

			var source = new Source(0, null, "DialogueManager");

			Game1.GlobalScope.SetVariable(source, "dialogueSelectorIndex", new Optional<int>(0));
            
            Game1.sceneManager.RunScene("UpdateSelectorPosition", source);

			UIManager.SetObjectVisible("Dialogue.Options", true);

			UISystem.SetAttributeChange("Dialogue.Options.Option1.text", "text", new Optional<string>(currentDialogueNode.Options[0].Text));

			if (currentDialogueNode.Options.Length < 2)
			{
				UIManager.SetObjectVisible("Dialogue.Options.Option2", false);
				UIManager.SetObjectVisible("Dialogue.Options.Option3", false);
				return;
			}

			UISystem.SetAttributeChange("Dialogue.Options.Option2.text", "text", new Optional<string>(currentDialogueNode.Options[1].Text));
			UIManager.SetObjectVisible("Dialogue.Options.Option2", true);

			if (currentDialogueNode.Options.Length < 3)
			{
				UIManager.SetObjectVisible("Dialogue.Options.Option3", false);
				return;
			}

			UISystem.SetAttributeChange("Dialogue.Options.Option3.text", "text", new Optional<string>(currentDialogueNode.Options[2].Text));
			UIManager.SetObjectVisible("Dialogue.Options.Option3", true);
			return;
		}

		UIManager.SetObjectVisible("Dialogue.Options", false);
	}

	private void SetSpeakerBoxVisibility(bool visible)
	{
		UIManager.SetObjectVisible("Dialogue.SpeakerBox", visible);
	}

	public void Update(GameTime gameTime)
	{
		if (!Active)
		{
			return;
		}

		var keystate = Keyboard.GetState();
        
        if (!_previousKeyState.HasValue)
        {
	        _previousKeyState = keystate;
	        return;
        }

		var current_step = currentDialogueNode!;

		_dialogueTimer += (float)gameTime.ElapsedGameTime.TotalSeconds * (current_step.SpeedMultiplier ?? 1f);

		int character_index = (int)Math.Min(Math.Ceiling(_dialogueTimer!.Value / TIME_PER_CHARACTER), current_step.text.Length);

		if (character_index > _currentCharacterIndex)
		{
			_currentCharacterIndex = character_index;

			UISystem.SetAttributeChange("Dialogue.DialogueBox.text", "text", new Optional<string>(current_step.text[..(int)Math.Min(Math.Ceiling(_dialogueTimer!.Value / TIME_PER_CHARACTER), current_step.text.Length)]));
		}

		// skip to end of current dialogue node
		if (!currentDialogueNode!.PreventSkip &&
			((_previousKeyState.Value.IsKeyDown(Keys.X) && keystate.IsKeyUp(Keys.X)) ||
			(_previousKeyState.Value.IsKeyDown(Keys.RightShift) && keystate.IsKeyUp(Keys.RightShift)) || 
			(_previousKeyState.Value.IsKeyDown(Keys.LeftShift) && keystate.IsKeyUp(Keys.LeftShift)))) 
		{
			_dialogueTimer = currentDialogueDuration;
		}

		// go to next dialogue
		if ((!_previousKeyState.Value.IsKeyDown(Keys.Z) || !keystate.IsKeyUp(Keys.Z)) &&
		    (!_previousKeyState.Value.IsKeyDown(Keys.Enter) || !keystate.IsKeyUp(Keys.Enter)))
		{
			_previousKeyState = keystate;
			return;
		}
        
		if (_dialogueTimer >= currentDialogueDuration &&
		    !currentDialogueTree!.AtEnd &&
		    currentDialogueTree.TryNext(new Source(0, null, "DialogueManager")))
		{	
			_currentCharacterIndex = 0;
			_dialogueTimer = 0f;

			UpdateDialogue();
		}

		if (currentDialogueTree!.AtEnd)
		{
			EndDialogue();
		}

		_previousKeyState = keystate;
	}
}