using Entropy.ECS;
using Entropy.Misc;

namespace Entropy.Scripting;

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
		function = (_, args, source) =>
		{
			var name = args[0].ToOptional<string>();
			var x = args[1].ToOptional<RelativeVariable<float>>();
			var y = args[2].ToOptional<RelativeVariable<float>>();

			if (!name.HasValue || !x.HasValue || !y.HasValue)
			{
				return null;
			}

			CharacterSystem.SetCharacterPosition(name.Value, Game1.tiledManager.TilePosToWorldPos(x.Value, y.Value));

			return null;
		}
	};
}