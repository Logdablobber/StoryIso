using Entropy.Misc;

namespace Entropy.Scripting;

static partial class FunctionDefs
{
	/// <summary>
	/// ToggleCollider
	/// <para>Takes in the collider's name as a string and toggles whether or not it's active.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef ToggleCollider = new()
	{
		name = "ToggleCollider",
		parameters = [typeof(string)],
		function = (_, args, source) =>
		{
			var collider_name = args[0].ToOptional<string>();

			if (!collider_name.HasValue)
			{
				return null;
			}

			Game1.tiledManager.currentRoom?.ToggleCollider(collider_name.Value, source!);
			return null;
		}
	};
}