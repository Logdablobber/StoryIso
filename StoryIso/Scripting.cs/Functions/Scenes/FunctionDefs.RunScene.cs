using StoryIso.Misc;

namespace StoryIso.Scripting;

static partial class FunctionDefs
{
	/// <summary>
	/// RunScene
	/// <para>Takes in the scene's name as a string and runs that scene.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef RunScene = new()
	{
		name = "RunScene",
		parameters = [typeof(string)],
		function = (_, args, source) => 
		{
			Optional<string> scene_name = ParameterProcessor.Convert<string>(source, args![0]);

			if (!scene_name.HasValue)
			{
				return null;
			}
		
			Game1.sceneManager.RunScene(scene_name.Value, source!);
			return null;
		}
	};
}