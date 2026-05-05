using Entropy.Misc;

namespace Entropy.Scripting;

static partial class FunctionDefs
{
	/// <summary>
	/// SetCollider
	/// <para>Takes in the collider's name as a string and the new state as a bool and sets the corresponding collider to that state.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef SetCollider = new()
	{
		name = "SetCollider",
		parameters = [typeof(string), typeof(bool)],
		function = (_, args, source) => 
		{
			var collider_name = args[0].ToOptional<string>();
			var state = args[1].ToOptional<bool>();

			if (!collider_name.HasValue || !state.HasValue)
			{
				return null;
			}

			Game1.tiledManager.currentRoom?.SetCollider(collider_name.Value, state.Value, source);
			return null;
		}
	};
}