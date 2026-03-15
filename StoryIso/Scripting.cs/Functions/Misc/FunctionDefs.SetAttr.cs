using System;
using StoryIso.ECS;

namespace StoryIso.Scripting;

static partial class FunctionDefs
{
	/// <summary>
	/// SetAttr
	/// <para>Takes in the target's name and attribute as string and the value. Sets an element of a given character or UI elements.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef SetAttr = new()
	{
		name = "SetAttr",
		parameters = [typeof(string), typeof(string), typeof(VariableObject)],
		function = (args, source) => 
		{
			var item1 = ParameterProcessor.Convert<string>(args![0]);
			var item2 = ParameterProcessor.Convert<string>(args[1]);
			var item3 = ParameterProcessor.ConvertUnknown(args[2], out _, out Type type);

			if (!item1.HasValue || !item2.HasValue || item3 == null)
			{
				return null;
			}

			CharacterSystem.SetAttribute(source!, item1.Value, item2.Value, item3, type);
			return null;
		}
	};
}