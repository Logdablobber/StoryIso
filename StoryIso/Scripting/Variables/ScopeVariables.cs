using System.Collections.Generic;
using StoryIso.Debugging;
using StoryIso.Enums;
using StoryIso.Misc;

namespace StoryIso.Scripting.Variables;

public class ScopeVariables
{
	private readonly Dictionary<string, IVariable> _variables = [];

	private ScopeVariables? parent;

	public ScopeVariables(ScopeVariables? parent, Dictionary<string, IVariable> variables)
	{
		this.parent = parent;
		this._variables = variables;
	}

	public IOptional GetVariable(Source source, string name, VariableType type, out bool is_constant)
	{
		// TODO: Improve?

		if (!_variables.TryGetValue(name, out var variable))
		{
			if (parent == null)
			{
				DebugConsole.Raise(new UndefinedVariableError(source, name));
				is_constant = default;
				return new Optional<string>();
			}

			var value = parent.GetVariable(source, name, type, out is_constant);

			if (!value.HasValue)
			{
				return new Optional<string>();
			}

			return value;
		}

		is_constant = variable.IsConstant;

		return ParameterProcessor.ConvertOptional(source, variable.Value, type);
	}

	public void SetVariable(Source source, string name, IOptional value)
	{
		if (!_variables.TryGetValue(name, out var variable))
		{
			if (parent == null)
			{
				DebugConsole.Raise(new UndefinedVariableError(source, name));
				return;
			}

			parent.SetVariable(source, name, value);
			return;
		}

		var converted_value = ParameterProcessor.ConvertOptional(source, value, VariableManager.GetVariableType(variable.ValueType));

		if (!converted_value.HasValue) 
		{
			return;
		}

		variable.Set(source, converted_value);
	}
}