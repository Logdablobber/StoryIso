using Entropy.Misc;

namespace Entropy.Scripting;

static partial class FunctionDefs
{
	/// <summary>
	/// RunDialogue
	/// <para>Takes in the dialogue's name as a string and runs that sequence.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef RunDialogue = new()
	{
		name = "RunDialogue",
		parameters = [typeof(string)],
		function = (_, args, source) => 
		{
			var dialogue_name = args[0].ToOptional<string>();

			if (!dialogue_name.HasValue)
			{
				return null;
			}

			Game1.sceneManager.dialogueManager.RunDialogue(dialogue_name.Value, source!);
			return null;
		}
	};
}