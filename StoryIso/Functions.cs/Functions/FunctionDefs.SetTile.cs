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
	/// SetTile
	/// <para>Takes in the tile layer, tile GUID as a uint, and two ints (x, y) and sets the tile at (x, y) to the tile referenced by the GUID on the given layer.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef SetTile = new()
	{
		name = "SetTile",
		parameters = [typeof(TileLayerType), typeof(uint), typeof(int), typeof(int)],
		function = (args, _) => 
		{
			var layer_type = ParameterProcessor.Convert<TileLayerType>(args![0]);
			Optional<uint> guid = ParameterProcessor.Convert<uint>(args[1]);
			Optional<int> x = ParameterProcessor.Convert<int>(args[2]);
			Optional<int> y = ParameterProcessor.Convert<int>(args[3]);

			if (layer_type.Value == TileLayerType.None || !guid.HasValue || !x.HasValue || !y.HasValue || x.Value < 0 || y.Value < 0)
			{
				return null;
			}
		
			Game1.tiledManager.currentRoom?.SetTile((ushort)x.Value, (ushort)y.Value, guid.Value, layer_type.Value);
			return null;
		}
	};
}