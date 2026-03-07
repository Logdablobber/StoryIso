using StoryIso.Misc;

namespace StoryIso.Functions;

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
		function = (args, source) => 
		{
			Optional<string> dialogue_name = ParameterProcessor.Convert<string>(args![0]);

			if (!dialogue_name.HasValue)
			{
				return null;
			}

			Game1.sceneManager.dialogueManager.RunDialogue(dialogue_name.Value[1..^1], source!);
			return null;
		}
	};
}