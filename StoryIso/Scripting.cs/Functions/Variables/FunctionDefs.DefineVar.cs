using StoryIso.Enums;
using StoryIso.Misc;

namespace StoryIso.Scripting;

static partial class FunctionDefs
{
	/// <summary>
	/// DefineVar
	/// <para>Defines a variable, takes in the type, name of the variable as an object, and the value of that type.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef DefineVar = new()
	{
		name = "DefineVar",
		parameters = [typeof(VariableType), typeof(object), typeof(VariableObject)], // the last value is string because it will be parsed later
		function = (args, source) => 
		{
			// variables are defined at startup
			Optional<string> name = ParameterProcessor.Convert<string>(args![1]);
			object? value = ParameterProcessor.ConvertUnknown(args[2]);

			if (!name.HasValue || value == null)
			{
				return null;
			}
		
			VariableManager.SetVariable(name.Value, value, source!);
			return null;
		}
	};
}