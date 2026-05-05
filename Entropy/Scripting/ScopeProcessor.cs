using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Assimp;
using Entropy.Debugging;
using Entropy.Enums;
using Entropy.Misc;
using Entropy.Scripting.Variables;

namespace Entropy.Scripting;

public static class ScopeProcessor
{
	private static uint? Run(Scope scope, ScopeVariables? variables, string obj, Source source, uint? gotoLine)
	{
		uint? goto_line = gotoLine;

		var current_variables = scope.CopyVariables(variables);

		for (int i = 0; i < scope.Objects.Count; i++)
		{
			var script_object = scope.Objects[i];

			if (script_object.IsScope)
			{
				if (script_object is not Scope current_scope)
				{
					throw new UnreachableException();
				}

				goto_line = Run(current_scope, current_variables, obj, source, goto_line);
			}

			else
			{
				if (script_object is not Function func)
				{
					throw new UnreachableException();
				}

				goto_line = RunFunct(current_variables, func,
					new Source(func.Line, FunctionDefs.Get(func.functionIndex).name, obj, source));
			}

			if (!goto_line.HasValue || goto_line.Value == script_object.Line)
			{
				continue;
			}
            
            if (CalcGotoIndex(scope, goto_line.Value, out i))
            {
	            return goto_line;
            }

			i--;
		}

		return null;
	}
    
    private static bool CalcGotoIndex(Scope scope, uint goto_line, out int index)
	{
		if (scope.Objects.Count == 0)
		{
			index = default;
			return true;
		}
        
		// if the goto line is outside of the scope
		if (goto_line > scope.Objects[^1].Line || goto_line < scope.Line)
		{
			index = default;
			return true;
		}

		// if the goto line would go to the first object
        if (goto_line <= scope.Objects[0].Line)
        {
	        index = 0;
	        return false;
        }
        
        // if the goto line would go to the last object
        if (goto_line == scope.Objects[^1].Line)
        {
	        index = scope.Objects.Count - 1;
	        return false;
        }
        
        // binary search
        var half_count = scope.Objects.Count >> 1; // divide by 2
        index = half_count;
        
        while (true)
		{
			// NOTE: index will never be 0
			if (scope.Objects[index].Line >= goto_line && scope.Objects[index - 1].Line < goto_line)
			{
				return false;
			}
            
            if (half_count != 1)
            {
	            half_count >>= 1; // divide by 2
            }
            
            if (scope.Objects[index].Line <= goto_line)
            {
	            index += half_count;
	            continue;
            }

            index -= half_count;
		}
       
	}

	public static void RunScope(Scope scope, string obj, Source source, bool sync = false, bool is_scene = false)
	{
		if (scope.Objects.Count == 0)
		{
			return;
		}

		if (sync)
		{
			Run(scope, Game1.GlobalScope.CopyVariables(null), obj, source, null);
			return;
		}

		Task.Run(() =>
		{
			if (is_scene)
			{
				Game1.sceneManager.Active = true;
			}

			Run(scope, Game1.GlobalScope.CopyVariables(null), obj, source, null);

			if (is_scene)
			{
				Game1.sceneManager.Active = false;
			}
		});
	}

	private static uint? RunFunct(ScopeVariables scope, Function func, Source source)
	{
		FunctionDef functionDef = FunctionDefs.Get(func.functionIndex);

		if (functionDef == null)
		{
			throw new UnreachableException();
		}

		IOptional[] parameters = new IOptional[functionDef.parameters.Length];

		for (int i = 0; i < functionDef.parameters.Length; i++)
		{
			void convert<T>() where T : notnull
			{
				parameters[i] = ParameterProcessor.Convert<T>(source, func.parameters[i]);
			}

			void convert_array<T>() where T : notnull
			{
				parameters[i] = ParameterProcessor.ArrayConvert<T>(source, func.parameters[i]);
			}

			byte type_indexer = TypeIndexers.GetTypeIndexer(functionDef.parameters[i]);
			switch (type_indexer)
			{
				case TypeIndexers.INT:
					convert<int>();
					break;

				case TypeIndexers.FLOAT:
					convert<float>();
					break;

				case TypeIndexers.OBJECT:
				case TypeIndexers.STRING:
					convert<string>();
					break;

				case TypeIndexers.BOOL:
					convert<bool>();
					break;

				case TypeIndexers.TILE_LAYER_TYPE:
					convert<TileLayerType>();
					break;

				case TypeIndexers.USHORT:
					convert<ushort>();
					break;

				case TypeIndexers.UINT:
					convert<uint>();
					break;

				case TypeIndexers.BYTE:
					convert<byte>();
					break;

				case TypeIndexers.RELATIVE_INT:
					parameters[i] = ParameterProcessor.RelativeConvert<int>(source, func.parameters[i]);
					break;

				case TypeIndexers.RELATIVE_FLOAT:
					parameters[i] = ParameterProcessor.RelativeConvert<float>(source, func.parameters[i]);
					break;

				case TypeIndexers.TYPE:
					convert<VariableType>();
					break;

				case TypeIndexers.VARIABLE_OBJECT:
					parameters[i] = ParameterProcessor.ConvertUnknown(source, func.parameters[i]);
					break;

				case TypeIndexers.DIRECTION:
					convert<Direction>();
					break;

				case TypeIndexers.ANIMATION:
					convert<AnimationType>();
					break;

				case >= 0b1000_0000:
					// remove array marker

					switch (type_indexer & 0b01111111)
					{
						case TypeIndexers.INT:
							convert_array<int>();
							break;

						case TypeIndexers.FLOAT:
							convert_array<float>();
							break;

						case TypeIndexers.STRING:
							convert_array<string>();
							break;

						case TypeIndexers.BOOL:
							convert_array<bool>();
							break;

						case TypeIndexers.USHORT:
							convert_array<ushort>();
							break;

						case TypeIndexers.UINT:
							convert_array<uint>();
							break;

						case TypeIndexers.BYTE:
							convert_array<byte>();
							break;

						default:
							throw new NotImplementedException();
					}

					break;

				default:
					throw new NotImplementedException();
			}
		}

		return functionDef.function!(scope, parameters, source);
	}
}