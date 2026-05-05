using System;
using Entropy.Debugging;
using Entropy.Enums;

namespace Entropy.Scripting;

public interface IFunctionParameter
{
	public bool IsConstant { get; }
	public Type ValueType { get; }
	public VariableType variableType { get; }

	public static IFunctionParameter Create(IFunctionParameter[] parameters, OperatorDef oper, Source source)
	{
		IFunctionParameter create<T1>() where T1 : notnull
		{
			return new FunctionParameter<T1>(new EquationTree<T1>(parameters, oper), source);
		}

		if (oper.returnType == typeof(int))
		{
			return create<int>();
		}

		if (oper.returnType == typeof(float))
		{
			return create<float>();
		}

		if (oper.returnType == typeof(string))
		{
			return create<string>();
		}

		if (oper.returnType == typeof(bool))
		{
			return create<bool>();
		}

		throw new NotImplementedException();
	}

	public abstract FunctionParameter<T1>? ConvertTo<T1>(Source source, Scope scope) where T1 : notnull;
}