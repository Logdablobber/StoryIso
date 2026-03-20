using Microsoft.Xna.Framework;
using StoryIso.ECS;
using StoryIso.Misc;

namespace StoryIso.Scripting;

static partial class FunctionDefs
{
	/// <summary>
	/// Move
	/// <para>Takes in two floats (x, y) and moves the player in that direction in tiny increments. This should be called every frame.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef Move = new()
	{
		name = "Move",
		parameters = [typeof(float), typeof(float)],
		function = (_, args, source) => 
		{
			Optional<float> x = ParameterProcessor.Convert<float>(source, args![0]);
			Optional<float> y = ParameterProcessor.Convert<float>(source, args[1]);
			if (!x.HasValue || !y.HasValue)
			{
				return null;
			}
		
			PlayerSystem.ApplyVelocity(new Vector2(x.Value, y.Value));
			return null;
		}
	};
}