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
			var layer_type = ParameterProcessor.Convert<TileLayerType>(source, args![0]);
			Optional<uint> guid = ParameterProcessor.Convert<uint>(source, args[1]);
			Optional<int> x = ParameterProcessor.Convert<int>(source, args[2]);
			Optional<int> y = ParameterProcessor.Convert<int>(source, args[3]);

			if (layer_type.Value == TileLayerType.None || !guid.HasValue || !x.HasValue || !y.HasValue || x.Value < 0 || y.Value < 0)
			{
				return null;
			}
		
			Game1.tiledManager.currentRoom?.SetTile((ushort)x.Value, (ushort)y.Value, guid.Value, layer_type.Value);
			return null;
		}
	};
}