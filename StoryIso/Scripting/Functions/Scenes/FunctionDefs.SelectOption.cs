using StoryIso.Misc;

namespace StoryIso.Scripting;

static partial class FunctionDefs
{
	/// <summary>
	/// Select Option
	/// <para>Selects an option in a dialogue, takes in am int for the option index. This should not be run normally.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef SelectOption = new()
	{
		name = "SelectOption",
		parameters = [typeof(int)],
		function = (_, args, source) => 
		{
			var index = args[0].ToOptional<int>();

			if (!index.HasValue)
			{
				return null;
			}

			Game1.sceneManager.dialogueManager.SelectDialogueOption(index.Value, source);
			return null;
		}
	};
}