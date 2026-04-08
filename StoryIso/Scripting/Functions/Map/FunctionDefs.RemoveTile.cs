using StoryIso.Enums;
using StoryIso.Misc;

namespace StoryIso.Scripting;

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
		function = (_, args, source) => 
		{
			var layer_type = args[0].ToOptional<TileLayerType>();
			var x = args[1].ToOptional<int>();
			var y = args[2].ToOptional<int>();

			if (layer_type.Value == TileLayerType.None || !x.HasValue || !y.HasValue || x.Value < 0 || y.Value < 0)
			{
				return null;
			}
		
			Game1.tiledManager.currentRoom?.SetTile((ushort)x.Value, (ushort)y.Value, 0, layer_type.Value);
			return null;
		}
	};
}