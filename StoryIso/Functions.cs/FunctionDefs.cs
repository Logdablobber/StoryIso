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
				Optional<string> arg = FunctionProcessor.Convert<string>(args![0]);

				if (!arg.HasValue)
				{
					return null;
				}

				DebugConsole.WriteLine(arg.Value[1..^1], Color.Black);

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
				var x = FunctionProcessor.RelativeConvert<float>(args![0]);
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
				var item1 = FunctionProcessor.Convert<Direction>(args![0]);

				if (item1.Value == Direction.None)
				{
					return null;
				}

				CharacterSystem.SetPlayerDirection(item1.Value);
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
				Optional<float> x = FunctionProcessor.Convert<float>(args![0]);
				Optional<float> y = FunctionProcessor.Convert<float>(args[1]);
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
				var item1 = FunctionProcessor.RelativeConvert<float>(args![0]);
				var item2 = FunctionProcessor.RelativeConvert<float>(args[1]);
				Optional<float> item3 = FunctionProcessor.Convert<float>(args[2]);

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
				Optional<string> map_name = FunctionProcessor.Convert<string>(args![0]);
				var x = FunctionProcessor.RelativeConvert<int>(args[1]);
				var y = FunctionProcessor.RelativeConvert<int>(args[2]);

				if (!map_name.HasValue || !x.HasValue || !y.HasValue)
				{
					return null;
				}

				Game1.PauseRendering();
				Game1.tiledManager.LoadMapThread(map_name.Value[1..^1]);
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
				var layer_type = FunctionProcessor.Convert<TileLayerType>(args![0]);
				Optional<uint> guid = FunctionProcessor.Convert<uint>(args[1]);
				Optional<ushort> x = FunctionProcessor.Convert<ushort>(args[2]);
				Optional<ushort> y = FunctionProcessor.Convert<ushort>(args[3]);

				if (layer_type.Value == TileLayerType.None || !guid.HasValue || !x.HasValue || !y.HasValue)
				{
					return null;
				}
			
				Game1.tiledManager.currentRoom.SetTile(x.Value, y.Value, guid.Value, layer_type.Value);
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
				var item1 = FunctionProcessor.Convert<TileLayerType>(args![0]);
				Optional<ushort> item2 = FunctionProcessor.Convert<ushort>(args[1]);
				Optional<ushort> item3 = FunctionProcessor.Convert<ushort>(args[2]);
				var item4 = FunctionProcessor.ArrayConvert<uint>(args[3]);

				if (item1.Value == TileLayerType.None || !item2.HasValue || !item3.HasValue || item4 == null)
				{
					return null;
				}
			
				for (int i = 0; i < item4.Length; i++)
				{
					Game1.tiledManager.currentRoom.SetTile((ushort)(item2.Value + i), item3.Value, item4[i], item1.Value);
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
				var item1 = FunctionProcessor.Convert<TileLayerType>(args![0]);
				Optional<ushort> item2 = FunctionProcessor.Convert<ushort>(args[1]);
				Optional<ushort> item3 = FunctionProcessor.Convert<ushort>(args[2]);
				var item4 = FunctionProcessor.ArrayConvert<uint>(args[3]);

				if (item1.Value == TileLayerType.None || !item2.HasValue || !item3.HasValue || item4 == null)
				{
					return null;
				}
			
				for (int i = 0; i < item4.Length; i++)
				{
					Game1.tiledManager.currentRoom.SetTile(item2.Value, (ushort)(item3.Value + i), item4[i], item1.Value);
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
				var layer_type = FunctionProcessor.Convert<TileLayerType>(args![0]);
				Optional<ushort> x = FunctionProcessor.Convert<ushort>(args[1]);
				Optional<ushort> y = FunctionProcessor.Convert<ushort>(args[2]);

				if (layer_type.Value == TileLayerType.None || !x.HasValue || !y.HasValue)
				{
					return null;
				}
			
				Game1.tiledManager.currentRoom.SetTile(x.Value, y.Value, 0, layer_type.Value);
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
				Optional<string> collider_name = FunctionProcessor.Convert<string>(args![0]);

				if (!collider_name.HasValue)
				{
					return null;
				}

				Game1.tiledManager.currentRoom.ToggleCollider(collider_name.Value[1..^1], source!);
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
				Optional<string> collider_name = FunctionProcessor.Convert<string>(args![0]);
				Optional<bool> state = FunctionProcessor.Convert<bool>(args[1]);

				if (!collider_name.HasValue || !state.HasValue)
				{
					return null;
				}

				Game1.tiledManager.currentRoom.SetCollider(collider_name.Value[1..^1], state.Value, source!);
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
				Optional<string> dialogue_name = FunctionProcessor.Convert<string>(args![0]);

				if (!dialogue_name.HasValue)
				{
					return null;
				}

				Game1.sceneManager.dialogueManager.RunDialogue(dialogue_name.Value[1..^1], source!);
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
				Optional<string> scene_name = FunctionProcessor.Convert<string>(args![0]);

				if (!scene_name.HasValue)
				{
					return null;
				}
			
				Game1.sceneManager.RunScene(scene_name.Value[1..^1], source!);
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
				// variables are defined at startup
				Optional<string> name = FunctionProcessor.Convert<string>(args![1]);
				object? value = FunctionProcessor.ConvertUnknown(args[2]);

				if (!name.HasValue || value == null)
				{
					return null;
				}
			
				VariableManager.SetVariable(name.Value, value, source!);
				return null;
			}
		},
		new FunctionDef // SetVar 
		{
			name = "SetVar",
			type = FunctionType.SetVar,
			parameters = [typeof(object), typeof(VariableObject)], // the last value is string because it will be parsed later
			function = (args, source) => 
			{
				Optional<string> name = FunctionProcessor.Convert<string>(args![0]);
				object? value = FunctionProcessor.ConvertUnknown(args[1]);

				if (!name.HasValue || value == null)
				{
					return null;
				}
			
				VariableManager.SetVariable(name.Value, value, source!);
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
				Optional<uint> line = FunctionProcessor.Convert<uint>(args![0]);

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
				var item1 = (PostfixEquation<bool>)args![0];
				Optional<uint> item2 = FunctionProcessor.Convert<uint>(args[1]);

				if (item1 == null || !item2.HasValue)
				{
					return null;
				}

				if (!item1.Evaluate(source!, out Optional<bool> result) || !result.HasValue || result.Value)
				{
					return null;
				}

				return item2.Value;
			}
		},
		new FunctionDef // SetVisible
		{
			name = "SetVisible",
			type = FunctionType.SetVisible,
			parameters = [typeof(string), typeof(bool)],
			function = (args, _) =>
			{
				Optional<string> item1 = FunctionProcessor.Convert<string>(args![0]);
				Optional<bool> item2 = FunctionProcessor.Convert<bool>(args[1]);

				if (!item1.HasValue || !item2.HasValue)
				{
					return null;
				}

				CharacterSystem.SetCharacterVisibility(item1.Value, item2.Value);

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
				Optional<string> item1 = FunctionProcessor.Convert<string>(args![0]);
				var item2 = FunctionProcessor.RelativeConvert<float>(args[1]);
				var item3 = FunctionProcessor.RelativeConvert<float>(args[2]);
				Optional<float> item4 = FunctionProcessor.Convert<float>(args[3]);

				if (!item1.HasValue || !item2.HasValue || !item3.HasValue || !item4.HasValue)
				{
					return null;
				}

				Movement movement = new Movement
				{
					movement = Game1.tiledManager.TilePosToWorldPos(item2.Value, item3.Value),
					speed = item4.Value
				};

				CharacterSystem.MoveCharacter(item1.Value, movement);

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
				Optional<string> item1 = FunctionProcessor.Convert<string>(args![0]);
				var item2 = FunctionProcessor.RelativeConvert<float>(args[1]);
				var item3 = FunctionProcessor.RelativeConvert<float>(args[2]);

				if (!item1.HasValue || !item2.HasValue || !item3.HasValue)
				{
					return null;
				}

				CharacterSystem.SetCharacterPosition(item1.Value, Game1.tiledManager.TilePosToWorldPos(item2.Value, item3.Value));

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
				var item1 = FunctionProcessor.Convert<string>(args![0]);
				var item2 = FunctionProcessor.Convert<Direction>(args[1]);

				if (!item1.HasValue || item2.Value == Direction.None)
				{
					return null;
				}

				CharacterSystem.SetCharacterDirection(item1.Value, item2.Value);
				return null;
			}
		},
		new FunctionDef // SetCharacterRoom
		{
			name = "SetCharacterRoom",
			type = FunctionType.SetCharacterRoom,
			parameters = [typeof(string), typeof(string)],
			function = (args, _) => 
			{
				var item1 = FunctionProcessor.Convert<string>(args![0]);
				var item2 = FunctionProcessor.Convert<string>(args[1]);

				if (!item1.HasValue || !item2.HasValue)
				{
					return null;
				}

				CharacterSystem.SetCharacterRoom(item1.Value, item2.Value);
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
				Optional<float> item1 = FunctionProcessor.Convert<float>(args![0]);

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
		new FunctionDef // SetAttr
		{
			name = "SetAttr",
			type = FunctionType.SetAttr,
			parameters = [typeof(string), typeof(string), typeof(VariableObject)],
			function = (args, source) => 
			{
				var item1 = FunctionProcessor.Convert<string>(args![0]);
				var item2 = FunctionProcessor.Convert<string>(args[1]);
				var item3 = FunctionProcessor.ConvertUnknown(args[2], out _, out Type type);

				if (!item1.HasValue || !item2.HasValue || item3 == null)
				{
					return null;
				}

				CharacterSystem.SetAttribute(source!, item1.Value, item2.Value, item3, type);
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
		JObject? jsonObj = JsonConvert.DeserializeObject(json) as JObject;

		if (jsonObj == null)
		{
			return;
		}

		JToken? token = jsonObj.SelectToken("repository.keywords.patterns");

		if (token == null)
		{
			return;
		}

		foreach (JToken pattern in token) 
		{
			switch (pattern.Value<string>("name"))
			{
				case "keyword":
					var match1 = pattern.Value<string>("match");
					pattern.SelectToken("match")?.Replace(match1 + $"|\\b({string.Join('|', VariableManager.KeywordNames)})\\b");
					break;

				case "entity.name.function":
					var match2 = pattern.Value<string>("match");
					pattern.SelectToken("match")?.Replace(match2 + $"\\b({string.Join('|', NameToFunctionDefIndex.Keys)})\\b");
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