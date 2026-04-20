using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Toolkit.HighPerformance;
using StoryIso.Debugging;
using StoryIso.Enums;
using StoryIso.Misc;
using StoryIso.Scripting.Variables;

namespace StoryIso.Scripting;

public class Scope : IScriptObject 
{
	private readonly uint _line;
	public uint Line
	{
		get 
		{
			return _line;
		}
	}

	private readonly uint _endLine;

	public bool IsScope
	{
		get
		{
			return true;
		}
	}

	public readonly List<IScriptObject> Objects;

	private Scope? parent;

	private readonly Dictionary<string, IVariable> _variables = [];

	public Scope(Scope? parent, IScriptObject[] objects, uint line, uint end_line)
	{
		this.parent = parent;
		this._line = line;
		this.Objects = objects.ToList();
		this._endLine = end_line;
	}

	public ScopeVariables CopyVariables(ScopeVariables? parent)
	{
		return new ScopeVariables(parent, _variables);
	}

	public IOptional GetVariable(Source source, string name, VariableType type, out bool is_constant)
	{
		if (!_variables.TryGetValue(name, out var variable))
		{
			if (parent == null)
			{
				DebugConsole.Raise(new UndefinedVariableError(source, name));
				is_constant = false;
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

	// finds the highest scope at the line you're at and checks for the variable
	public bool ContainsVariable(string name, uint line, out VariableType type, out bool is_constant)
	{
		return GetCurrentScope(line).ContainsVariable(name, out type, out is_constant);
	}

	public bool ContainsVariable(string name, uint line, out VariableType type)
	{
		return ContainsVariable(name, line, out type, out _);
	}

	// check the scope your currently in, does not try to find a higher scope
	public bool ContainsVariable(string name, out VariableType type, out bool is_constant)
	{
		if (_variables.TryGetValue(name, out var variable))
		{
			type = VariableManager.GetVariableType(variable.ValueType);
			is_constant = variable.IsConstant;
			return true;
		}

		if (parent == null)
		{
			type = VariableType.None;
			is_constant = default;
			return false;
		}

		return parent.ContainsVariable(name, out type, out is_constant);
	}

	public bool ContainsVariable(string name, out VariableType type)
	{
		return ContainsVariable(name, out type, out _);
	}

	public bool IsLocalVariable(string name, uint line)
	{
		var scope = GetCurrentScope(line);

		return scope._variables.ContainsKey(name);
	}

	public Scope GetCurrentScope(uint line)
	{
		for (int i = 0; i < Objects.Count; i++)
		{
			if (!Objects[i].IsScope)
			{
				continue;
			}

			if (Objects[i].Line > line)
			{
				continue;
			}

			if (Objects[i] is not Scope scope)
			{
				throw new UnreachableException();
			}

			if (scope._endLine < line)
			{
				continue;
			}

			return scope.GetCurrentScope(line);
		}

		return this;
	}

	public void DefineVariable(Source source, IVariable variable)
	{
		if (_variables.ContainsKey(variable.Name))
		{
			DebugConsole.Raise(new VariableAlreadyExistsError(source, variable.Name));
			return;
		}

		_variables.Add(variable.Name, variable);
	}

	public void ClearVariables()
	{
		_variables.Clear();
	}

	public void SetParent(Scope parent)
	{
		this.parent = parent;
	}

	public bool AddObject(Source source, IScriptObject obj)
	{
		if (obj.Line < this.Line || obj.Line > this._endLine)
		{
			return false;
		}

		if (Objects.Count == 0)
		{
			if (obj.IsScope)
			{
				(obj as Scope)?.SetParent(this);
			}

			Objects.Add(obj);
			return true;
		}

		for (int i = Objects.Count - 1; i >= 0; i--)
		{
			if (Objects[i].Line > obj.Line)
			{
				continue;
			}

			if (!Objects[i].IsScope)
			{
				if (obj.IsScope)
				{
					(obj as Scope)?.SetParent(this);
				}

				Objects.Insert(i + 1, obj);
				return true;
			}

			if (Objects[i] is not Scope scope)
			{
				throw new UnreachableException();
			}

			if (!scope.AddObject(source, obj))
			{
				if (obj.IsScope)
				{
					(obj as Scope)?.SetParent(this);
				}

				this.Objects.Add(obj);
			}
			return true;
		}

		return false;
	}
}