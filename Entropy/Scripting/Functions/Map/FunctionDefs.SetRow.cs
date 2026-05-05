using Entropy.Enums;
using Entropy.Misc;

namespace Entropy.Scripting;

static partial class FunctionDefs
{
	/// <summary>
	/// SetRow
	/// <para>Takes in the tile layer, two ints (x, y) and an array of uints as the GUIDs and sets the tiles starting at (x, y) to the tile referenced by the GUIDs on the given layer.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef SetRow = new()
	{
		name = "SetRow",
		parameters = [typeof(TileLayerType), typeof(int), typeof(int), typeof(uint[])],
		function = (_, args, source) => 
		{
			var layer_type = args[0].ToOptional<TileLayerType>();
			var x = args[1].ToOptional<int>();
			var y = args[2].ToOptional<int>();
			var guids = args[3].ToOptional<uint[]>();

			if (layer_type.Value == TileLayerType.None || !x.HasValue || !y.HasValue || !guids.HasValue || x.Value < 0 || y.Value < 0)
			{
				return null;
			}
		
			for (int i = 0; i < guids.Value.Length; i++)
			{
				Game1.tiledManager.currentRoom?.SetTile((ushort)(x.Value + i), (ushort)y.Value, guids.Value[i], layer_type.Value);
			}
			
			return null;
		}
	};
}