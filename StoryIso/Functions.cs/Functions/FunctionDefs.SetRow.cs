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
	/// SetRow
	/// <para>Takes in the tile layer, two ints (x, y) and an array of uints as the GUIDs and sets the tiles starting at (x, y) to the tile referenced by the GUIDs on the given layer.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef SetRow = new()
	{
		name = "SetRow",
		parameters = [typeof(TileLayerType), typeof(int), typeof(int), typeof(uint[])],
		function = (args, _) => 
		{
			var item1 = ParameterProcessor.Convert<TileLayerType>(args![0]);
			Optional<int> item2 = ParameterProcessor.Convert<int>(args[1]);
			Optional<int> item3 = ParameterProcessor.Convert<int>(args[2]);
			var item4 = ParameterProcessor.ArrayConvert<uint>(args[3]);

			if (item1.Value == TileLayerType.None || !item2.HasValue || !item3.HasValue || item4 == null || item2.Value < 0 || item3.Value < 0)
			{
				return null;
			}
		
			for (int i = 0; i < item4.Length; i++)
			{
				Game1.tiledManager.currentRoom?.SetTile((ushort)(item2.Value + i), (ushort)item3.Value, item4[i], item1.Value);
			}
			
			return null;
		}
	};
}