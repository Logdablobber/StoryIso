using Entropy.Misc;

namespace Entropy.Scripting;

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
		parameters = [typeof(object), typeof(VariableObject)],
		function = (scope, args, source) => 
		{
			var name = args[0].ToOptional<string>();
			var value = args[1];

			if (!name.HasValue)
			{
				return null;
			}
		
			scope.SetVariable(source, name.Value, value);
			return null;
		}
	};
}