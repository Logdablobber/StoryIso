using StoryIso.Misc;

namespace StoryIso.Scripting;

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
		function = (args, source) => 
		{
			Optional<string> collider_name = ParameterProcessor.Convert<string>(args![0]);
			Optional<bool> state = ParameterProcessor.Convert<bool>(args[1]);

			if (!collider_name.HasValue || !state.HasValue)
			{
				return null;
			}

			Game1.tiledManager.currentRoom?.SetCollider(collider_name.Value[1..^1], state.Value, source!);
			return null;
		}
	};
}