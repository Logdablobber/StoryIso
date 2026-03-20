using System.Diagnostics;
using StoryIso.Enums;
using StoryIso.Misc;
using StoryIso.Scripting.Variables;

namespace StoryIso.Scripting;
/*
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
		function = (scope, args, source) => 
		{
			Optional<VariableType> type = ParameterProcessor.Convert<VariableType>(source, args![0]);
			Optional<string> name = ParameterProcessor.Convert<string>(source, args[1]);
			IOptional? value = ParameterProcessor.ConvertUnknown(source, args[2]);

			if (!type.HasValue || !name.HasValue)
			{
				return null;
			}

			IVariable create<T>() where T : notnull
			{
				if (value == null)
				{
					return new ValueVariable<T>(name.Value, default);
				}

				return new ValueVariable<T>(name.Value, ParameterProcessor.ConvertOptional<T>(source, value));
			}

			var new_variable = type.Value switch
			{
				VariableType.Int => create<int>(),
				VariableType.Float => create<float>(),
				VariableType.String => create<string>(),
				VariableType.Bool => create<bool>(),
				_ => throw new UnreachableException(),
			};
		
			scope.DefineVariable(source, new_variable);
			return null;
		}
	};
}*/