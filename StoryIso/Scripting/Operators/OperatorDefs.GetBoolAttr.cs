using StoryIso.Misc;
using System;
using StoryIso.ECS;

namespace StoryIso.Scripting;

static partial class OperatorDefs
{
	/// <summary>
	/// Get Bool Attribute
	/// <para>takes in the name of the target and the attribute as strings and returns the value of that attribute as a bool</para>
	/// </summary>
	/// 
	/// <returns>bool</returns>
	private static readonly OperatorDef GetBoolAttr = new()
	{
		oper = "GetBoolAttr",
		inlineFunc = true,
		isConstant = false,
        sync = true,
		parameters = [typeof(string), typeof(string)],
		returnType = typeof(bool),
		function = (args, source) =>
		{
			var target = args[0].ToOptional<string>();
			var attr = args[1].ToOptional<string>();

			if (!target.HasValue || !attr.HasValue)
			{
				return new Optional<bool>();
			}

			return ParameterProcessor.ConvertOptional<bool>(source, CharacterSystem.GetAttribute(source, target.Value, attr.Value));
		}
	};
}