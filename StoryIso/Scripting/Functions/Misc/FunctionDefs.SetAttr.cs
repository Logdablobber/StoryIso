using System;
using StoryIso.ECS;
using StoryIso.Misc;

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
		function = (_, args, source) => 
		{
			var name = args[0].ToOptional<string>();
			var attr = args[1].ToOptional<string>();
			var value = args[2];

			if (!name.HasValue || !attr.HasValue || !value.HasValue)
			{
				return null;
			}

			CharacterSystem.SetAttribute(source!, name.Value, attr.Value, value);
			return null;
		}
	};
}