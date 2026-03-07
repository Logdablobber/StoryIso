using StoryIso.Misc;

namespace StoryIso.Functions;

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
		function = (args, source) =>
		{
			Optional<string> collider_name = ParameterProcessor.Convert<string>(args![0]);

			if (!collider_name.HasValue)
			{
				return null;
			}

			Game1.tiledManager.currentRoom?.ToggleCollider(collider_name.Value[1..^1], source!);
			return null;
		}
	};
}