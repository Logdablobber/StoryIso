using System;
using System.Threading;
using Entropy.Misc;

namespace Entropy.Scripting;

static partial class FunctionDefs
{
	/// <summary>
	/// Wait for dialogue end
	/// <para>Pauses the script execution until the current dialogue ends. The game still runs while the script is paused.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef WaitForDialogueEnd = new()
	{
		name = "WaitForDialogueEnd",
		parameters = [],
		function = (_, _, source) =>
		{
			while (Game1.sceneManager.dialogueManager.Active)
			{
				Thread.Sleep(10);
			}

			return null;
		}
	};
}