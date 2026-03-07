using StoryIso.ECS;
using StoryIso.Misc;

namespace StoryIso.Functions;

static partial class FunctionDefs
{
	/// <summary>
	/// SetPlayerPos
	/// <para>Takes in two relative floats (x, y) and sets the player's position to that</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef SetPlayerPos = new()
	{
		name = "SetPlayerPos",
		parameters = [typeof(RelativeVariable<float>), typeof(RelativeVariable<float>)],
		function = (args, _) => 
		{
			var x = ParameterProcessor.RelativeConvert<float>(args![0]);
			var y = ParameterProcessor.RelativeConvert<float>(args[1]);

			if (!x.HasValue || !y.HasValue)
			{
				return null;
			}

			CharacterSystem.SetPlayerPosition(Game1.tiledManager.TilePosToWorldPos(x.Value, y.Value));
			return null;
		}
	};
}