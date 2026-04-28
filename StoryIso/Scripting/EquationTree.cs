using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StoryIso.Debugging;
using StoryIso.Misc;

namespace StoryIso.Scripting;

public class EquationTree<T> where T : notnull
{
	private readonly IFunctionParameter[] parameters;
	private readonly OperatorDef? operatorDef;
	private Optional<T> _tempReturn;
	private readonly Lock _tempReturnLock = new();

	private Source? _tempSource;
	private Task _updateTemp;

	public bool IsConstant
	{
		get
		{
			if (operatorDef == null)
			{
				return true;
			}

			return operatorDef.isConstant && parameters.All(parameter => parameter.IsConstant);
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
		this._tempSource = new Source(0, null, "Equation Tree Startup");
		this._updateTemp = Task.Run(UpdateTemp);
	}

	public EquationTree(IFunctionParameter value)
	{
		this.parameters = [value];
		this.operatorDef = null;
	}
    
    private void UpdateTemp()
	{
		if (_tempSource == null)
		{
			throw new NullReferenceException();
		}

		var res = Evaluate(_tempSource, true);

		lock (_tempReturnLock)
		{
			_tempReturn = res;
		}
	}

	public Optional<T> Evaluate(Source source, bool override_sync = false)
	{
		if (!override_sync && !(operatorDef?.sync ?? true))
		{
			Optional<T> res;
			lock (_tempReturnLock)
			{
				res = _tempReturn;
			}
            
            if (_updateTemp.Status != TaskStatus.Running)
            {
	            _tempSource = source;
	            _updateTemp = Task.Run(UpdateTemp);
            }
            
			return res;
		}
        
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
		if (operatorDef != null)
		{
			return new EquationTree<T1>(parameters, operatorDef);
		}

		if (parameters.Length != 1)
		{
			throw new UnreachableException();
		}

		return new EquationTree<T1>(parameters[0]);
	}
}