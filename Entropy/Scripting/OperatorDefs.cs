using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Entropy.Scripting;

public static partial class OperatorDefs
{
	public static readonly Dictionary<string, OperatorDef> OperatorsFromString = [];
	public static string[] Operators = null!;
	public static string[] InlineFunctions = null!;

	public static Regex OperatorRegex = null!;

	public static void Initialize()
	{
		// link all of the operators defined in ./Functions/Operators/
		// using System.Reflection!
		var fields = typeof(OperatorDefs).GetFields(BindingFlags.NonPublic | BindingFlags.Static).Where((field) => field.FieldType == typeof(OperatorDef)).ToArray();

		foreach (var field in fields)
		{
			var value = field.GetValue(null);

			if (value == null)
			{
				continue;
			}

			var def_value = (OperatorDef)value;

			OperatorsFromString.Add(def_value.oper, def_value);
		}

		InlineFunctions = OperatorsFromString.Where((oper) => oper.Value.inlineFunc).ToDictionary().Keys.ToArray();
		Operators = OperatorsFromString.Where((oper) => !oper.Value.inlineFunc).ToDictionary().Keys.ToArray();
		HashSet<char> characters = new HashSet<char>();
		foreach (var oper in Operators)
		{
			characters.UnionWith(oper);
		}

		characters.Remove('-'); // '-' has to be at end b/c of regex stuff

		var regex_string = $"[{string.Join("", from c in characters select $"{c}")}-]|{string.Join("|", InlineFunctions)}";
		Debug.WriteLine(regex_string);

		OperatorRegex = new Regex(regex_string, RegexOptions.Compiled | RegexOptions.NonBacktracking);
	}

	public static OperatorDef Get(string name)
	{
		return OperatorsFromString[name];
	}
}