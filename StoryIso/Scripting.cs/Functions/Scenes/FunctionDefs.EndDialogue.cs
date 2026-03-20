namespace StoryIso.Scripting;

static partial class FunctionDefs
{
	/// <summary>
	/// EndDialogue
	/// <para>Stops the currently playing dialogue sequence, if it exists.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef EndDialogue = new()
	{
		name = "EndDialogue",
		parameters = [],
		function = (_, _, source) => 
		{
			Game1.sceneManager.dialogueManager.EndDialogue(source);
			return null;
		}
	};
}