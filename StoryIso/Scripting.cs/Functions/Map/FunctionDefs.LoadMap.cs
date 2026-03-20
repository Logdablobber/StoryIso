using StoryIso.ECS;
using StoryIso.Misc;

namespace StoryIso.Scripting;

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
		function = (_, args, source) => 
		{
			Optional<string> map_name = ParameterProcessor.Convert<string>(source, args![0]);
			var x = ParameterProcessor.RelativeConvert<float>(source, args[1]);
			var y = ParameterProcessor.RelativeConvert<float>(source, args[2]);

			if (!map_name.HasValue || !x.HasValue || !y.HasValue)
			{
				return null;
			}

			Game1.PauseRendering();
			Game1.tiledManager.LoadMapThread(source, map_name.Value);
			CharacterSystem.SetPlayerPosition(Game1.tiledManager.TilePosToWorldPos(x.Value, y.Value));
			return null;
		}
	};
}