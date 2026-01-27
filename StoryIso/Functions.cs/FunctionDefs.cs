using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StoryIso.Debugging;
using StoryIso.ECS;
using StoryIso.Enums;
using StoryIso.Misc;
using StoryIso.Scenes;

namespace StoryIso.Functions;

public static class FunctionDefs
{
	static readonly List<FunctionDef> _functions = new List<FunctionDef>()
	{
		new FunctionDef // None
		{
			name = null,
			type = FunctionType.None,
			parameters = [],
			function = null
		},
		new FunctionDef // Print 
		{
			name = "Print",
			type = FunctionType.Print,
			parameters = [typeof(string)],
			function = (args, _) => 
			{
				var arg = FunctionProcessor.Convert<string>(args[0]);

				if (arg == null)
				{
					return null;
				}

				DebugConsole.WriteLine(arg[1..^1], Color.Black);

				return null;
			}
		},
		new FunctionDef // SetPlayerPos 
		
		{
			name = "SetPlayerPos",
			type = FunctionType.SetPlayerPos,
			parameters = [typeof(RelativeVariable<float>), typeof(RelativeVariable<float>)],
			function = (args, _) => 
			{
				var x = FunctionProcessor.RelativeConvert<float>(args[0]);
				var y = FunctionProcessor.RelativeConvert<float>(args[1]);

				if (!x.HasValue || !y.HasValue)
				{
					return null;
				}

				CharacterSystem.SetPlayerPosition(Game1.tiledManager.TilePosToWorldPos(x.Value, y.Value));
				return null;
			}
		},
		new FunctionDef // SetPlayerDirection
		{
			name = "SetPlayerDirection",
			type = FunctionType.SetPlayerDirection,
			parameters = [typeof(Direction)],
			function = (args, _) => 
			{
				var item1 = FunctionProcessor.Convert<Direction>(args[0]);

				if (item1 == Direction.None)
				{
					return null;
				}

				CharacterSystem.SetPlayerDirection(item1);
				return null;
			}
		},
		new FunctionDef // Move 
		{
			name = "Move",
			type = FunctionType.Move,
			parameters = [typeof(float), typeof(float)],
			function = (args, _) => 
			{
				float? x = FunctionProcessor.Convert<float?>(args[0]);
				float? y = FunctionProcessor.Convert<float?>(args[1]);
				if (!x.HasValue || !y.HasValue)
				{
					return null;
				}
			
				PlayerSystem.ApplyVelocity(new Vector2(x.Value, y.Value));
				return null;
			}
		},
		new FunctionDef // MoveTo  
		{
			name = "MoveTo",
			type = FunctionType.MoveTo,
			parameters = [typeof(RelativeVariable<float>), typeof(RelativeVariable<float>), typeof(float)],
			function = (args, _) => 
			{
				var item1 = FunctionProcessor.RelativeConvert<float>(args[0]);
				var item2 = FunctionProcessor.RelativeConvert<float>(args[1]);
				var item3 = FunctionProcessor.Convert<float?>(args[2]);

				if (!item1.HasValue || !item2.HasValue || !item3.HasValue)
				{
					return null;
				}

				Movement movement = new Movement
				{
					movement = Game1.tiledManager.TilePosToWorldPos(item1.Value, item2.Value),
					speed = item3.Value
				};

				CharacterSystem.MovePlayer(movement);

				return null;
			}
		},
		new FunctionDef // LoadMap
		{
			name = "LoadMap",
			type = FunctionType.LoadMap,
			parameters = [typeof(string), typeof(RelativeVariable<int>), typeof(RelativeVariable<int>)],
			function = (args, _) => 
			{
				var map_name = FunctionProcessor.Convert<string>(args[0]);
				var x = FunctionProcessor.RelativeConvert<int>(args[1]);
				var y = FunctionProcessor.RelativeConvert<int>(args[2]);

				if (map_name == null || !x.HasValue || !y.HasValue)
				{
					return null;
				}

				Game1.PauseRendering();
				Game1.tiledManager.LoadMapThread(map_name[1..^1]);
				CharacterSystem.SetPlayerPosition(Game1.tiledManager.TilePosToWorldPos(x.Value, y.Value));
				return null;
			}
		},
		new FunctionDef // SetTile
		{
			name = "SetTile",
			type = FunctionType.SetTile,
			parameters = [typeof(TileLayerType), typeof(uint), typeof(ushort), typeof(ushort)],
			function = (args, _) => 
			{
				var layer_type = FunctionProcessor.Convert<TileLayerType>(args[0]);
				var guid = FunctionProcessor.Convert<uint?>(args[1]);
				var x = FunctionProcessor.Convert<ushort?>(args[2]);
				var y = FunctionProcessor.Convert<ushort?>(args[3]);

				if (layer_type == TileLayerType.None || !guid.HasValue || !x.HasValue || !y.HasValue)
				{
					return null;
				}
			
				Game1.tiledManager.currentRoom.SetTile(x.Value, y.Value, guid.Value, layer_type);
				return null;
			}
		},
		new FunctionDef // SetRow
		{
			name = "SetRow",
			type = FunctionType.SetRow,
			parameters = [typeof(TileLayerType), typeof(ushort), typeof(ushort), typeof(uint[])],
			function = (args, _) => 
			{
				var item1 = FunctionProcessor.Convert<TileLayerType>(args[0]);
				var item2 = FunctionProcessor.Convert<ushort?>(args[1]);
				var item3 = FunctionProcessor.Convert<ushort?>(args[2]);
				var item4 = FunctionProcessor.ArrayConvert<uint?>(args[3]);

				if (item1 == TileLayerType.None || !item2.HasValue || !item3.HasValue || item4 == null)
				{
					return null;
				}
			
				for (int i = 0; i < item4.Length; i++)
				{
					Game1.tiledManager.currentRoom.SetTile((ushort)(item2.Value + i), item3.Value, item4[i].Value, item1);
				}
				
				return null;
			}
		},
		new FunctionDef // SetCol
		{
			name = "SetCol",
			type = FunctionType.SetCol,
			parameters = [typeof(TileLayerType), typeof(ushort), typeof(ushort), typeof(uint[])],
			function = (args, _) => 
			{
				var item1 = FunctionProcessor.Convert<TileLayerType>(args[0]);
				var item2 = FunctionProcessor.Convert<ushort?>(args[1]);
				var item3 = FunctionProcessor.Convert<ushort?>(args[2]);
				var item4 = FunctionProcessor.ArrayConvert<uint?>(args[3]);

				if (item1 == TileLayerType.None || !item2.HasValue || !item3.HasValue || item4 == null)
				{
					return null;
				}
			
				for (int i = 0; i < item4.Length; i++)
				{
					Game1.tiledManager.currentRoom.SetTile(item2.Value, (ushort)(item3.Value + i), item4[i].Value, item1);
				}
				
				return null;
			}
		},
		new FunctionDef // RemoveTile 
		{
			name = "RemoveTile",
			type = FunctionType.RemoveTile,
			parameters = [typeof(TileLayerType), typeof(ushort), typeof(ushort)],
			function = (args, _) => 
			{
				var layer_type = FunctionProcessor.Convert<TileLayerType>(args[0]);
				var x = FunctionProcessor.Convert<ushort?>(args[1]);
				var y = FunctionProcessor.Convert<ushort?>(args[2]);

				if (layer_type == TileLayerType.None || !x.HasValue || !y.HasValue)
				{
					return null;
				}
			
				Game1.tiledManager.currentRoom.SetTile(x.Value, y.Value, 0, layer_type);
				return null;
			}
		},
		new FunctionDef // ToggleCollider
		{
			name = "ToggleCollider",
			type = FunctionType.ToggleCollider,
			parameters = [typeof(string)],
			function = (args, source) =>
			{
				string collider_name = FunctionProcessor.Convert<string>(args[0]);

				if (collider_name == null)
				{
					return null;
				}

				Game1.tiledManager.currentRoom.ToggleCollider(collider_name[1..^1], source);
				return null;
			}
		},
		new FunctionDef // SetCollider 
		{
			name = "SetCollider",
			type = FunctionType.SetCollider,
			parameters = [typeof(string), typeof(bool)],
			function = (args, source) => 
			{
				string collider_name = FunctionProcessor.Convert<string>(args[0]);
				bool? state = FunctionProcessor.Convert<bool?>(args[1]);

				if (collider_name == null || !state.HasValue)
				{
					return null;
				}

				Game1.tiledManager.currentRoom.SetCollider(collider_name[1..^1], state.Value, source);
				return null;
			}
		},
		new FunctionDef // RefreshMap 
		{
			name = "RefreshMap",
			type = FunctionType.RefreshMap,
			parameters = [],
			function = (_, _) => 
			{
				Game1.tiledManager.RefreshMapThread();
				return null;
			}
		},
		new FunctionDef // RunDialogue 
		{
			name = "RunDialogue",
			type = FunctionType.RunDialogue,
			parameters = [typeof(string)],
			function = (args, source) => 
			{
				string dialogue_name = FunctionProcessor.Convert<string>(args[0]);

				if (dialogue_name == null)
				{
					return null;
				}

				Game1.sceneManager.dialogueManager.RunDialogue(dialogue_name[1..^1], source);
				return null;
			}
		},
		new FunctionDef // EndDialogue 
		{
			name = "EndDialogue",
			type = FunctionType.EndDialogue,
			parameters = [],
			function = (_, source) => 
			{
				Game1.sceneManager.dialogueManager.EndDialogue(source);
				return null;
			}
		},
		new FunctionDef // RunScene 
		{
			name = "RunScene",
			type = FunctionType.RunScene,
			parameters = [typeof(string)],
			function = (args, source) => 
			{
				string scene_name = FunctionProcessor.Convert<string>(args[0]);

				if (scene_name == null)
				{
					return null;
				}
			
				Game1.sceneManager.RunScene(scene_name[1..^1], source);
				return null;
			}
		},
		new FunctionDef // DefineVar 
		{
			name = "DefineVar",
			type = FunctionType.DefineVar,
			parameters = [typeof(VariableType), typeof(object), typeof(VariableObject)], // the last value is string because it will be parsed later
			function = (args, source) => 
			{
				var type = FunctionProcessor.Convert<VariableType>(args[0]);
				var name = FunctionProcessor.Convert<string>(args[1]);
				var value = FunctionProcessor.ConvertUnknown(args[2]);

				if (name == null || value == null || type == VariableType.Bool)
				{
					return null;
				}
			
				VariableManager.DefineVariable(type, name, value, source);
				return null;
			}
		},
		new FunctionDef // SetVar 
		{
			name = "SetVar",
			type = FunctionType.SetVar,
			parameters = [typeof(object), typeof(object)], // the last value is string because it will be parsed later
			function = (args, source) => 
			{
				string name = FunctionProcessor.Convert<string>(args[0]);
				string value = FunctionProcessor.Convert<string>(args[1]);

				if (name == null || value == null)
				{
					return null;
				}
			
				VariableManager.SetVariable(name, value, source);
				return null;
			}
		},
		new FunctionDef // GOTO 
		{
			name = "GOTO",
			type = FunctionType.GOTO,
			parameters = [typeof(uint)],
			function = (args, source) => 
			{
				uint? line = FunctionProcessor.Convert<uint?>(args[0]);

				if (!line.HasValue)
				{
					return null;
				}

				return line.Value;
			}
		},
		new FunctionDef // GOTOIF
		{
			name = null,
			type = FunctionType.GOTOIF,
			parameters = [typeof(object), typeof(uint)],
			function = (args, source) =>
			{
				var item1 = (PostfixEquation<bool?>)args[0];
				var item2 = FunctionProcessor.Convert<uint?>(args[1]);

				if (item1 == null || !item2.HasValue)
				{
					return null;
				}

				if (!item1.Evaluate(source, out bool? result) || !result.HasValue || !result.Value)
				{
					return null;
				}

				return item2;
			}
		},
		new FunctionDef // SetVisible
		{
			name = "SetVisible",
			type = FunctionType.SetVisible,
			parameters = [typeof(string), typeof(bool)],
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<string>(args[0]);
				var item2 = FunctionProcessor.Convert<bool?>(args[1]);

				if (item1 == null || !item2.HasValue)
				{
					return null;
				}

				CharacterSystem.SetCharacterVisibility(item1, item2.Value);

				return null;
			}
		},
		new FunctionDef // MoveCharacter
		{
			name = "MoveCharacter",
			type = FunctionType.MoveCharacter,
			parameters = [typeof(string), typeof(RelativeVariable<float>), typeof(RelativeVariable<float>), typeof(float)],
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<string>(args[0]);
				var item2 = FunctionProcessor.RelativeConvert<float>(args[1]);
				var item3 = FunctionProcessor.RelativeConvert<float>(args[2]);
				var item4 = FunctionProcessor.Convert<float?>(args[3]);

				if (item1 == null || !item2.HasValue || !item3.HasValue || !item4.HasValue)
				{
					return null;
				}

				Movement movement = new Movement
				{
					movement = Game1.tiledManager.TilePosToWorldPos(item2.Value, item3.Value),
					speed = item4.Value
				};

				CharacterSystem.MoveCharacter(item1, movement);

				return null;
			}
		},
		new FunctionDef // SetCharacterPos
		{
			name = "SetCharacterPos",
			type = FunctionType.SetCharacterPos,
			parameters = [typeof(string), typeof(RelativeVariable<float>), typeof(RelativeVariable<float>)],
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<string>(args[0]);
				var item2 = FunctionProcessor.RelativeConvert<float>(args[1]);
				var item3 = FunctionProcessor.RelativeConvert<float>(args[2]);

				if (item1 == null || !item2.HasValue || !item3.HasValue)
				{
					return null;
				}

				CharacterSystem.SetCharacterPosition(item1, Game1.tiledManager.TilePosToWorldPos(item2.Value, item3.Value));

				return null;
			}
		},
		new FunctionDef // SetCharacterDirection
		{
			name = "SetCharacterDirection",
			type = FunctionType.SetCharacterDirection,
			parameters = [typeof(string), typeof(Direction)],
			function = (args, _) => 
			{
				var item1 = FunctionProcessor.Convert<string>(args[0]);
				var item2 = FunctionProcessor.Convert<Direction>(args[1]);

				if (item1 == null || item2 == Direction.None)
				{
					return null;
				}

				CharacterSystem.SetCharacterDirection(item1, item2);
				return null;
			}
		},
		new FunctionDef // Wait
		{
			name = "Wait",
			type = FunctionType.Wait,
			parameters = [typeof(float)],
			function = (args, _) =>
			{
				var item1 = FunctionProcessor.Convert<float?>(args[0]);

				if (!item1.HasValue)
				{
					return null;
				}

				Thread.Sleep(TimeSpan.FromSeconds(item1.Value));

				return null;
			}
		},
		new FunctionDef // UnlockPlayerMovement
		{
			name = "UnlockPlayerMovement",
			type = FunctionType.UnlockPlayerMovement,
			parameters = [],
			function = (_, _) =>
			{
				Game1.sceneManager.Active = false;

				return null;
			}
		},
		new FunctionDef // LockPlayerMovement
		{
			name = "LockPlayerMovement",
			type = FunctionType.LockPlayerMovement,
			parameters = [],
			function = (_, _) =>
			{
				Game1.sceneManager.Active = true;

				return null;
			}
		},
	};

	private static Dictionary<string, int> NameToFunctionDefIndex = [];
	private static Dictionary<FunctionType, int> TypeToFunctionDefIndex = [];

	public static void Initialize()
	{
		for (int i = 0; i < _functions.Count; i++)
		{
			FunctionDef def = _functions[i];

			if (def.type == FunctionType.None)
			{
				continue;
			}

			if (def.name != null)
			{
				NameToFunctionDefIndex.Add(def.name, i);
			}
			TypeToFunctionDefIndex.Add(def.type, i);
		}

		#if DEBUG
		// update scene formatting extension with variable and function names

		string user_folder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		string extension_dir = user_folder + "\\.vscode\\extensions\\scene-syntax-highlighting\\syntaxes\\";

		string json = File.ReadAllText(extension_dir + "scene.tmLanguageBase.json");
		JObject jsonObj = JsonConvert.DeserializeObject(json) as JObject;

		foreach (JToken pattern in jsonObj.SelectToken("repository.keywords.patterns")) 
		{
			switch (pattern.Value<string>("name"))
			{
				case "keyword":
					var match1 = pattern.Value<string>("match");
					pattern.SelectToken("match").Replace(match1 + $"|\\b({string.Join('|', VariableManager.KeywordNames)})\\b");
					break;

				case "entity.name.function":
					var match2 = pattern.Value<string>("match");
					pattern.SelectToken("match").Replace(match2 + $"\\b({string.Join('|', NameToFunctionDefIndex.Keys)})\\b");
					break;

				default:
					break;
			}
		}

		string updated_json_string = jsonObj.ToString(Formatting.Indented);

		if (File.Exists(extension_dir + "scene.tmLanguage.json"))
		{
			File.Delete(extension_dir + "scene.tmLanguage.json");
		}

		File.WriteAllText(extension_dir + "scene.tmLanguage.json", updated_json_string);
		#endif
	}


	public static FunctionDef Get(string name)
	{
		if (NameToFunctionDefIndex.TryGetValue(name, out var value))
		{
			return _functions[value];
		}

		return _functions[0]; // null function
	}

	public static FunctionDef Get(FunctionType type)
	{
		if (TypeToFunctionDefIndex.TryGetValue(type, out var value))
		{
			return _functions[value];
		}

		return _functions[0]; // null function
	}
}