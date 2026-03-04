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
	/// RemoveTile
	/// <para>Takes in the tile layer and two ints (x, y) and sets the tile at (x, y) to GUID 0, aka nothing.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef RemoveTile = new()
	{
		name = "RemoveTile",
		parameters = [typeof(TileLayerType), typeof(int), typeof(int)],
		function = (args, _) => 
		{
			var layer_type = ParameterProcessor.Convert<TileLayerType>(args![0]);
			Optional<int> x = ParameterProcessor.Convert<int>(args[1]);
			Optional<int> y = ParameterProcessor.Convert<int>(args[2]);

			if (layer_type.Value == TileLayerType.None || !x.HasValue || !y.HasValue || x.Value < 0 || y.Value < 0)
			{
				return null;
			}
		
			Game1.tiledManager.currentRoom?.SetTile((ushort)x.Value, (ushort)y.Value, 0, layer_type.Value);
			return null;
		}
	};
}