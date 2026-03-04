using System;
using System.Threading;
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
	/// Wait
	/// <para>Pauses the script execution for a given amount of time in seconds. The game still runs while the script is paused.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef Wait = new()
	{
		name = "Wait",
		parameters = [typeof(float)],
		function = (args, _) =>
		{
			Optional<float> item1 = ParameterProcessor.Convert<float>(args![0]);

			if (!item1.HasValue)
			{
				return null;
			}

			Thread.Sleep(TimeSpan.FromSeconds(item1.Value));

			return null;
		}
	};
}