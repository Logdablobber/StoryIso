using StoryIso.Enums;
using StoryIso.Misc;

namespace StoryIso.Scripting;

static partial class FunctionDefs
{
	/// <summary>
	/// SetTile
	/// <para>Takes in the tile layer, tile GUID as a uint, and two ints (x, y) and sets the tile at (x, y) to the tile referenced by the GUID on the given layer.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef SetTile = new()
	{
		name = "SetTile",
		parameters = [typeof(TileLayerType), typeof(uint), typeof(int), typeof(int)],
		function = (_, args, source) => 
		{
			var layer_type = args[0].ToOptional<TileLayerType>();
			var guid = args[1].ToOptional<uint>();
			var x = args[2].ToOptional<int>();
			var y = args[3].ToOptional<int>();

			if (layer_type.Value == TileLayerType.None || !guid.HasValue || !x.HasValue || !y.HasValue || x.Value < 0 || y.Value < 0)
			{
				return null;
			}
		
			Game1.tiledManager.currentRoom?.SetTile((ushort)x.Value, (ushort)y.Value, guid.Value, layer_type.Value);
			return null;
		}
	};
}