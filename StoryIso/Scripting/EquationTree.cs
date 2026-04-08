using System;
using System.Collections.Generic;
using System.Diagnostics;
using StoryIso.Debugging;
using StoryIso.Misc;

namespace StoryIso.Scripting;

public class EquationTree<T> where T : notnull
{
	private readonly IFunctionParameter[] parameters;
	private readonly OperatorDef? operatorDef;

	public bool IsConstant
	{
		get
		{
			if (operatorDef == null)
			{
				return true;
			}

			if (!operatorDef.isConstant)
			{
				return false;
			}

			foreach (var parameter in parameters)
			{
				if (!parameter.IsConstant)
				{
					return false;
				}
			}

			return true;
		}
	}

	public EquationTree(IFunctionParameter[] parameters, OperatorDef operatorDef)
	{
		if (operatorDef.function == null)
		{
			throw new NullReferenceException();
		}

		this.parameters = parameters;
		this.operatorDef = operatorDef;
	}

	public EquationTree(IFunctionParameter value)
	{
		this.parameters = [value];
		this.operatorDef = null;
	}

	public Optional<T> Evaluate(Source source)
	{
		if (operatorDef == null)
		{
			if (parameters.Length != 1)
			{
				throw new UnreachableException();
			}

			return ParameterProcessor.ConvertParam<T>(source, parameters[0]);
		}

		List<IOptional> operator_params = [];

		for (int i = 0; i < parameters.Length; i++) 
		{
			var param_value = ParameterProcessor.ConvertParam(source, parameters[i], operatorDef.parameters[i]);

			if (param_value == null)
			{
				return default;
			}

			operator_params.Add(param_value);
		}

		var function_res = operatorDef.function!(operator_params, source);

		if (function_res == null)
		{
			return default;
		}

		return ParameterProcessor.ConvertOptional<T>(source, function_res);
	}

	public EquationTree<T1> ConvertTo<T1>() where T1 : notnull
	{
		if (operatorDef == null)
		{
			if (parameters.Length != 1)
			{
				throw new UnreachableException();
			}

			return new EquationTree<T1>(parameters[0]);
		}

		return new EquationTree<T1>(parameters, operatorDef);
	}
}