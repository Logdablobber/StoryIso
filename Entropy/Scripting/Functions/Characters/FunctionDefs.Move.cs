using Microsoft.Xna.Framework;
using Entropy.ECS;
using Entropy.Misc;

namespace Entropy.Scripting;

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
			var x = args[0].ToOptional<float>();
			var y = args[1].ToOptional<float>();
			
			if (!x.HasValue || !y.HasValue)
			{
				return null;
			}
		
			PlayerSystem.ApplyVelocity(new Vector2(x.Value, y.Value));
			return null;
		}
	};
}