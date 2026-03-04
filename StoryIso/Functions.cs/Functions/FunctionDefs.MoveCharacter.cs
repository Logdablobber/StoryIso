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
	/// MoveCharacter
	/// <para> Takes in a character's name as a string, a position (x, y) as relative floats, and a float speed where speed=1 is normal player speed. Move the character given to the point given smoothly at the given speed.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef MoveCharacter = new()
	{
		name = "MoveCharacter",
		parameters = [typeof(string), typeof(RelativeVariable<float>), typeof(RelativeVariable<float>), typeof(float)],
		function = (args, _) =>
		{
			Optional<string> item1 = ParameterProcessor.Convert<string>(args![0]);
			var item2 = ParameterProcessor.RelativeConvert<float>(args[1]);
			var item3 = ParameterProcessor.RelativeConvert<float>(args[2]);
			Optional<float> item4 = ParameterProcessor.Convert<float>(args[3]);

			if (!item1.HasValue || !item2.HasValue || !item3.HasValue || !item4.HasValue)
			{
				return null;
			}

			Movement movement = new Movement
			{
				movement = Game1.tiledManager.TilePosToWorldPos(item2.Value, item3.Value),
				speed = item4.Value
			};

			CharacterSystem.MoveCharacter(item1.Value, movement);

			return null;
		}
	};
}