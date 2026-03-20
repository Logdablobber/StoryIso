using StoryIso.ECS;
using StoryIso.Misc;
using StoryIso.Scenes;

namespace StoryIso.Scripting;

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
			var item1 = ParameterProcessor.RelativeConvert<float>(source, args![0]);
			var item2 = ParameterProcessor.RelativeConvert<float>(source, args[1]);
			Optional<float> item3 = ParameterProcessor.Convert<float>(source, args[2]);

			if (!item1.HasValue || !item2.HasValue || !item3.HasValue)
			{
				return null;
			}

			Movement movement = new Movement
			{
				movement = Game1.tiledManager.TilePosToWorldPos(item1.Value, item2.Value),
				speed = item3.Value
			};

			CharacterSystem.MovePlayer(movement);

			return null;
		}
	};
}