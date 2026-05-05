using Entropy.ECS;
using Entropy.Misc;
using Entropy.Scenes;

namespace Entropy.Scripting;

static partial class FunctionDefs
{
	/// <summary>
	/// MoveTo
	/// <para>Takes in two relative floats (x, y) and another float speed. Sets the player to move toward the location (x, y) at the speed multiplier where speed=1 is the base player speed.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef MoveTo = new()
	{
		name = "MoveTo",
		parameters = [typeof(RelativeVariable<float>), typeof(RelativeVariable<float>), typeof(float)],
		function = (_, args, source) => 
		{
			var x = args[0].ToOptional<RelativeVariable<float>>();
			var y = args[1].ToOptional<RelativeVariable<float>>();
			var speed = args[2].ToOptional<float>();

			if (!x.HasValue || !y.HasValue || !speed.HasValue)
			{
				return null;
			}

			Movement movement = new Movement
			{
				movement = Game1.tiledManager.TilePosToWorldPos(x.Value, y.Value),
				speed = speed.Value
			};

			CharacterSystem.MovePlayer(movement);

			return null;
		}
	};
}