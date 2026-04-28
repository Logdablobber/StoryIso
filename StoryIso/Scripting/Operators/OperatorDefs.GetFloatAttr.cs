using StoryIso.Misc;
using System;
using StoryIso.ECS;

namespace StoryIso.Scripting;

static partial class OperatorDefs
{
	/// <summary>
	/// Get Float Attribute
	/// <para>takes in the name of the target and the attribute as strings and returns the value of that attribute as a float</para>
	/// </summary>
	/// 
	/// <returns>float</returns>
	private static readonly OperatorDef GetFloatAttr = new()
	{
		oper = "GetFloatAttr",
		inlineFunc = true,
		isConstant = false,
        sync = false,
		parameters = [typeof(string), typeof(string)],
		returnType = typeof(float),
		function = (args, source) =>
		{
			var target = args[0].ToOptional<string>();
			var attr = args[1].ToOptional<string>();

			if (!target.HasValue || !attr.HasValue)
			{
				return new Optional<float>();
			}

			return ParameterProcessor.ConvertOptional<float>(source, CharacterSystem.GetAttribute(source, target.Value, attr.Value).ToOptional<float>());
		}
	};
}