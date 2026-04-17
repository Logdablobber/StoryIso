using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using StoryIso.Debugging;
using StoryIso.Dialogue;
using StoryIso.Misc;
using StoryIso.Scripting;

namespace StoryIso.Scenes;

public class SceneManager
{
	public DialogueManager dialogueManager;
	private Dictionary<string, Scene>? _scenes;

	private bool _active = false;
	public bool Active
	{
		get
		{
			return _active || dialogueManager.Active;
		}

		set
		{
			_active = value;
		}
	}

	public SceneManager(Texture2D dialogue_box, Texture2D name_box, Texture2D option_box, FontInstance font, string scene_directory)
	{
		dialogueManager = new DialogueManager(dialogue_box_texture: dialogue_box,
												name_box_texture: name_box,
												option_box_texture: option_box,
												scale: 1f,
												position: new Vector2(0, Game1.camera.BoundingRectangle.Height),
												font: font,
												font_scale: 0.25f,
												dialogue_directory: "./Content/Scenes/Dialogue");

		LoadScenes(scene_directory);
	}

	private void LoadScene(string obj, string[] scene_lines)
	{
		if (_scenes == null)
		{
			throw new NullReferenceException("_scenes is null :(");
		}

		Scene? new_scene = SceneProcessor.ProcessScene(obj, scene_lines);

		if (new_scene == null)
		{
			return;
		}

		_scenes.Add(new_scene.name, new_scene);
	}

	private void LoadScenes(string directory)
	{
		_scenes = [];

		DirectoryInfo dir = new DirectoryInfo(directory);

		FileInfo[] files = dir.GetFiles("*.scene");

		List<(string, string[])> scenes = [];
		
		foreach (var file in files)
		{
			string scene_text;
			using(StreamReader streamReader = new(file.FullName))
			{
				scene_text = streamReader.ReadToEnd();
			}

			string[] scene_lines = TextFormatter.SplitLines(scene_text);

			scenes.Add((file.FullName, scene_lines));

			SceneProcessor.PreprocessScene(file.FullName, scene_lines);
		}

		foreach (var (scene_name, scene_lines) in scenes)
		{
			LoadScene(scene_name, scene_lines);
		}
	}

	public void RunScene(string name, Source source)
	{
		if (_scenes == null)
		{
			throw new NullReferenceException("_scenes is null :(");
		}

		if (!_scenes.TryGetValue(name, out Scene? scene))
		{
			DebugConsole.Raise(new InvalidSceneError(source, name));
			return;
		}

		FunctionProcessor.RunScope(scene.scope, scene.name, source, is_scene: true);
	}

	public void Update(GameTime gameTime)
	{
		dialogueManager.Update(gameTime);
	}
}