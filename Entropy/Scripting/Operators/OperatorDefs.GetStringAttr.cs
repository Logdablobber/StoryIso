using Entropy.Misc;
using System;
using Entropy.ECS;

namespace Entropy.Scripting;

static partial class OperatorDefs
{
	/// <summary>
	/// Get String Attribute
	/// <para>takes in the name of the target and the attribute as strings and returns the value of that attribute as a string</para>
	/// </summary>
	/// 
	/// <returns>string</returns>
	private static readonly OperatorDef GetStringAttr = new()
	{
		oper = "GetStringAttr",
		inlineFunc = true,
		isConstant = false,
        sync = true,
		parameters = [typeof(string), typeof(string)],
		returnType = typeof(string),
		function = (args, source) =>
		{
			var target = args[0].ToOptional<string>();
			var attr = args[1].ToOptional<string>();

			if (!target.HasValue || !attr.HasValue)
			{
				return new Optional<string>();
			}

			return ParameterProcessor.ConvertOptional<string>(source, CharacterSystem.GetAttribute(source, target.Value, attr.Value));
		}
	};
}