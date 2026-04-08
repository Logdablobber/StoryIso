using System;
using System.Threading;
using StoryIso.Misc;

namespace StoryIso.Scripting;

static partial class FunctionDefs
{
	/// <summary>
	/// Wait
	/// <para>Pauses the script execution for a given amount of time in seconds. The game still runs while the script is paused.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef Wait = new()
	{
		name = "Wait",
		parameters = [typeof(float)],
		function = (_, args, source) =>
		{
			var time = args[0].ToOptional<float>();

			if (!time.HasValue)
			{
				return null;
			}

			Thread.Sleep(TimeSpan.FromSeconds(time.Value));

			return null;
		}
	};
}