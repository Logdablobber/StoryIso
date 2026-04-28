using StoryIso.Misc;
using System;
using StoryIso.ECS;

namespace StoryIso.Scripting;

static partial class OperatorDefs
{
	/// <summary>
	/// Get Int Attribute
	/// <para>takes in the name of the target and the attribute as strings and returns the value of that attribute as an int</para>
	/// </summary>
	/// 
	/// <returns>int</returns>
	private static readonly OperatorDef GetIntAttr = new()
	{
		oper = "GetIntAttr",
		inlineFunc = true,
		isConstant = false,
        sync = false,
		parameters = [typeof(string), typeof(string)],
		returnType = typeof(int),
		function = (args, source) =>
		{
			var target = args[0].ToOptional<string>();
			var attr = args[1].ToOptional<string>();

			if (!target.HasValue || !attr.HasValue)
			{
				return new Optional<int>();
			}

			return ParameterProcessor.ConvertOptional<int>(source, CharacterSystem.GetAttribute(source, target.Value, attr.Value));
		}
	};
}