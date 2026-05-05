using Entropy.Misc;

namespace Entropy.Scripting;

static partial class FunctionDefs
{
	/// <summary>
	/// RunSceneAsync
	/// <para>Takes in the scene's name as a string and runs that scene asyncronously.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef RunSceneAsync = new()
	{
		name = "RunSceneAsync",
		parameters = [typeof(string)],
		function = (_, args, source) => 
		{
			var scene_name = args[0].ToOptional<string>();

			if (!scene_name.HasValue)
			{
				return null;
			}
		
			Game1.sceneManager.RunScene(scene_name.Value, source);
			return null;
		}
	};
}