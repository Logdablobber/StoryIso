using Microsoft.Xna.Framework;
using StoryIso.Debugging;
using StoryIso.ECS;
using StoryIso.Enums;
using StoryIso.Misc;
using StoryIso.Scenes;

namespace StoryIso.Functions;

static partial class FunctionDefs
{
	/// <summary>
	/// LoadMap
	/// <para>Takes in a string as the map name and two relative floats (x, y). Sets the current map and player's position accordingly.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef LoadMap = new()
	{
		name = "LoadMap",
		parameters = [typeof(string), typeof(RelativeVariable<float>), typeof(RelativeVariable<float>)],
		function = (args, _) => 
		{
			Optional<string> map_name = ParameterProcessor.Convert<string>(args![0]);
			var x = ParameterProcessor.RelativeConvert<float>(args[1]);
			var y = ParameterProcessor.RelativeConvert<float>(args[2]);

			if (!map_name.HasValue || !x.HasValue || !y.HasValue)
			{
				return null;
			}

			Game1.PauseRendering();
			Game1.tiledManager.LoadMapThread(map_name.Value[1..^1]);
			CharacterSystem.SetPlayerPosition(Game1.tiledManager.TilePosToWorldPos(x.Value, y.Value));
			return null;
		}
	};
}