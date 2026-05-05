using Entropy.ECS;
using Entropy.Misc;

namespace Entropy.Scripting;

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
			var map_name = args[0].ToOptional<string>();
			var x = args[1].ToOptional<RelativeVariable<float>>();
			var y = args[2].ToOptional<RelativeVariable<float>>();

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