using Microsoft.Xna.Framework;
using StoryIso.Debugging;
using StoryIso.ECS;
using StoryIso.Enums;
using StoryIso.Misc;
using StoryIso.Scenes;

namespace StoryIso.Functions;

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
		function = (_, source) => 
		{
			Game1.sceneManager.dialogueManager.EndDialogue(source);
			return null;
		}
	};
}