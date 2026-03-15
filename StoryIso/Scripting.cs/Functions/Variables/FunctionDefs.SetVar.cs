using StoryIso.Misc;

namespace StoryIso.Scripting;

static partial class FunctionDefs
{
	/// <summary>
	/// SetVar
	/// <para> Sets a variable, takes in the variable's name as an object & new value.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef SetVar = new()
	{
		name = "SetVar",
		parameters = [typeof(object), typeof(VariableObject)], // the last value is string because it will be parsed later
		function = (args, source) => 
		{
			Optional<string> name = ParameterProcessor.Convert<string>(args![0]);
			object? value = ParameterProcessor.ConvertUnknown(args[1]);

			if (!name.HasValue || value == null)
			{
				return null;
			}
		
			VariableManager.SetVariable(name.Value, value, source!);
			return null;
		}
	};
}