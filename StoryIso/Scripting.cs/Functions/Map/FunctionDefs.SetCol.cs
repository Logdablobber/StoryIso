using System;
using StoryIso.Enums;
using StoryIso.Misc;

namespace StoryIso.Scripting;

static partial class FunctionDefs
{
	/// <summary>
	/// SetCol
	/// <para>Takes in the tile layer, two ints (x, y) and an array of uints as the GUIDs and sets the tiles starting at (x, y) to the tile referenced by the GUIDs on the given layer vertically.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef SetCol = new()
	{
		name = "SetCol",
		parameters = [typeof(TileLayerType), typeof(int), typeof(int), typeof(uint[])],
		function = (_, args, source) => 
		{
			var item1 = ParameterProcessor.Convert<TileLayerType>(source, args![0]);
			Optional<int> item2 = ParameterProcessor.Convert<int>(source, args[1]);
			Optional<int> item3 = ParameterProcessor.Convert<int>(source, args[2]);
			var item4 = ParameterProcessor.ArrayConvert<uint>(source, args[3]);

			if (item1.Value == TileLayerType.None || !item2.HasValue || !item3.HasValue || item4 == null || item2.Value < 0 || item3.Value < 0)
			{
				return null;
			}
		
			for (int i = 0; i < item4.Length; i++)
			{
				Game1.tiledManager.currentRoom?.SetTile((ushort)item2.Value, (ushort)(item3.Value + i), item4[i], item1.Value);
			}
			
			return null;
		}
	};
}