using StoryIso.ECS;
using StoryIso.Misc;

namespace StoryIso.Functions;

static partial class FunctionDefs
{
	/// <summary>
	/// SetCharacterPos
	/// <para> Takes in a character's name as a string and a position (x, y) as relative floats. Sets the given character's position to the given position.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef SetCharacterPos = new()
	{
		name = "SetCharacterPos",
		parameters = [typeof(string), typeof(RelativeVariable<float>), typeof(RelativeVariable<float>)],
		function = (args, _) =>
		{
			Optional<string> item1 = ParameterProcessor.Convert<string>(args![0]);
			var item2 = ParameterProcessor.RelativeConvert<float>(args[1]);
			var item3 = ParameterProcessor.RelativeConvert<float>(args[2]);

			if (!item1.HasValue || !item2.HasValue || !item3.HasValue)
			{
				return null;
			}

			CharacterSystem.SetCharacterPosition(item1.Value, Game1.tiledManager.TilePosToWorldPos(item2.Value, item3.Value));

			return null;
		}
	};
}