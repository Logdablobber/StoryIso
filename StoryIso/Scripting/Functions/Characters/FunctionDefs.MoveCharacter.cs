using StoryIso.ECS;
using StoryIso.Misc;
using StoryIso.Scenes;

namespace StoryIso.Scripting;

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
		function = (_, args, source) =>
		{
			var name = args[0].ToOptional<string>();
			var x = args[1].ToOptional<RelativeVariable<float>>();
			var y = args[2].ToOptional<RelativeVariable<float>>();
			var speed_mult = args[3].ToOptional<float>();

			if (!name.HasValue || !x.HasValue || !y.HasValue || !speed_mult.HasValue)
			{
				return null;
			}

			Movement movement = new Movement
			{
				movement = Game1.tiledManager.TilePosToWorldPos(x.Value, y.Value),
				speed = speed_mult.Value
			};

			CharacterSystem.MoveCharacter(name.Value, movement);

			return null;
		}
	};
}