using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Threading;
using StoryIso.Debugging;
using StoryIso.Enums;
using StoryIso.Misc;
using StoryIso.Scripting.Variables;

namespace StoryIso.Scripting;

public static partial class FunctionProcessor
{
	private static readonly Regex _splitRegex = SplitRegex();
	public static void Initialize()
	{
		FunctionDefs.Initialize();
		OperatorDefs.Initialize();
	}

	private static string[][] Tokenize(string[] input)
	{
		return (from line 
					in input
					select 
					(from match 
					in _splitRegex.Matches(line.Trim())
					where !string.IsNullOrWhiteSpace(match.Value)
					select match.Value.Trim()).ToArray()).ToArray();
	} 

	public static void Preprocess(string obj, string[] funcs_string, uint start_line = 0)
	{
		var lines = Tokenize(funcs_string);

		for (uint i = start_line; i < lines.Length; i++)
		{
			Source temp_source = new(i + 1, null, obj);

			var matches = lines[i];

			if (matches.Length == 0)
			{
				continue;
			}

			if (matches[0] != "var")
			{
				continue;
			} 

			if (matches.Length < 3)
			{
				continue;
			}
			
			var define_parameters = ParameterProcessor.ProcessParameters(temp_source, Game1.GlobalScope, "DefineVar", [matches[1], matches[2]], [typeof(VariableType), typeof(object)]);

			if (define_parameters == null)
			{
				continue; // TODO: raise error
			}

			define_parameters.Add(new FunctionParameter<string>());

			VariableManager.DefineVariable(temp_source, Game1.GlobalScope, define_parameters);
 		}
	}

	public static Scope? Process(string obj, string funcs_string, uint start_line = 0)
	{
		return Process(obj, TextFormatter.SplitLines(funcs_string), start_line);
	}

	public static Scope? Process(string obj, string[] funcs_string, uint start_line = 0)
	{
		Scope new_scope = new(Game1.GlobalScope, [], start_line, (uint)funcs_string.Length);

		var lines = Tokenize(funcs_string);

		bool inside_if = false;
		bool inside_else = false;

		Dictionary<string, uint> loops = [];

		for (uint i = start_line; i < lines.Length; i++)
		{
			Source temp_source = new(i + 1, null, obj);

			var matches = lines[i];

			if (matches.Length == 0)
			{
				continue;
			}

			string first_value = matches[0];

			if (first_value.StartsWith("//"))
			{
				continue; // comment
			}

			switch (first_value)
			{
				case "#IF":

					if (inside_if)
					{
						DebugConsole.Raise(new NestedIfError(temp_source));
						return null;
					}

					inside_if = true;

					string input = matches[1..].JoinToString();

					if (!ParameterEvaluator.ToNodeTree<bool>(temp_source, new_scope.GetCurrentScope(i), "IF", $"!({input})", out var if_condition))
					{
						continue;
					}

					uint? goto_line = null;

					for (uint j = i + 1; j < lines.Length; j++)
					{
						if (lines[j][0] == "#ENDIF" ||
							lines[j][0] == "#ELIF" ||
							lines[j][0] == "#ELSE")
						{
							goto_line = j;
							break;
						}
					}
					
					if (!goto_line.HasValue)
					{
						goto_line = (uint)lines.Length;
					}

					var if_scope = new Scope(null, [], i, goto_line.Value);

					if_scope.AddObject(temp_source, new Function(FunctionDefs.GOTOIF_Index, 
												[if_condition!, 
												new FunctionParameter<uint>(goto_line.Value)], 
											i));

					new_scope.AddObject(temp_source, if_scope);
					continue;

				case "#ELIF":
					if (!inside_if || inside_else)
					{
						DebugConsole.Raise(new MissingIfError(temp_source));
						return null;
					}

					string elif_input = matches[1..].JoinToString();

					if (!ParameterEvaluator.ToNodeTree<bool>(temp_source, new_scope.GetCurrentScope(i), "ELIF", $"!({elif_input})", out var elif_condition))
					{
						continue;
					}

					uint? elif_goto_line = null;
					uint? endif_line = null;

					for (uint j = i + 1; j < lines.Length; j++)
					{
						if (!elif_goto_line.HasValue &&
							(lines[j][0] == "#ENDIF" ||
							lines[j][0] == "#ELIF" ||
							lines[j][0] == "#ELSE"))
						{
							elif_goto_line = j;
						}

						if (lines[j][0] == "#ENDIF")
						{
							endif_line = j;

							if (!elif_goto_line.HasValue)
							{
								elif_goto_line = j;
							}

							break;
						}
					}
					
					if (!endif_line.HasValue || !elif_goto_line.HasValue)
					{
						endif_line = (uint)lines.Length;

						if (!elif_goto_line.HasValue)
						{
							elif_goto_line = endif_line;
						}
					}

					// this is the goto for the IF statement before it
					new_scope.AddObject(temp_source, new Function(FunctionDefs.GetIndex("GOTO"),
											[new FunctionParameter<uint>(endif_line.Value)],
											i - 1));

					var elif_scope = new Scope(null, [], i, elif_goto_line.Value);

					elif_scope.AddObject(temp_source, new Function(FunctionDefs.GOTOIF_Index, 
												[elif_condition!, 
												new FunctionParameter<uint>(elif_goto_line.Value)], 
											i));

					new_scope.AddObject(temp_source, elif_scope);
					continue;

				case "#ELSE":
					if (!inside_if || inside_else)
					{
						DebugConsole.Raise(new MissingIfError(temp_source));
						return null;
					}

					inside_else = true;

					uint? else_endif_line = null;

					for (uint j = i + 1; j < lines.Length; j++)
					{
						if (lines[j][0] == "#ENDIF")
						{
							else_endif_line = j;
							break;
						}
					}	

					if (!else_endif_line.HasValue)
					{
						else_endif_line = (uint)lines.Length;
					}		

					// for IF statements before this one
					new_scope.AddObject(temp_source, new Function(FunctionDefs.GetIndex("GOTO"),
											[new FunctionParameter<uint>(else_endif_line.Value)],
											i - 1));

					new_scope.AddObject(temp_source, new Scope(null, [], i, else_endif_line.Value));
					continue;
				
				case "#ENDIF":
					inside_if = false;
					inside_else = false;
					continue;

				case "#LOOP": // should be in format #LOOP LOOP_NAME AMOUNT_OF_LOOPS
					if (matches.Length < 3)
					{
						DebugConsole.Raise(new ParameterError(temp_source, "LOOP", matches.Length - 1, 2, "LOOP should be given a name and then the amount of cycles to run. Did you forget a comma?"));
						continue;
					}

					string loop_name = matches[1];

					uint end_loop_line = 0;

					for (uint j = i + 1; j < lines.Length; j++)
					{
						if (j == lines.Length - 1) 
						{
							end_loop_line = j;
							break;
						}

						if (lines[j].Length == 0)
						{
							continue;
						}

						if (lines[j][0] == $"#ENDLOOP {loop_name}")
						{
							end_loop_line = j;
							break;
						}
					}

					var loop_variable = new ValueVariable<int>(loop_name, 0);

					new_scope.GetCurrentScope(i).DefineVariable(temp_source, loop_variable);

					if (!ParameterEvaluator.ToNodeTree<bool>(temp_source, new_scope.GetCurrentScope(i), "LOOP", $"({loop_name} == {matches[2..].JoinToString()}) || ({loop_name} == -1)", out var condition))
					{
						continue;
					}
					
					new_scope.AddObject(temp_source, new Function(FunctionDefs.GetIndex("SetVar"),
											[new FunctionParameter<string>(value:loop_name), 
											new FunctionParameter<int>(0)], i - 1));

					var loop_scope = new Scope(null, [], i, end_loop_line);

					loop_scope.AddObject(temp_source, new Function(FunctionDefs.GOTOIF_Index,
											[condition!, 
											new FunctionParameter<uint>(end_loop_line)], i));
					
					new_scope.AddObject(temp_source, loop_scope);

					loops.Add(loop_name, i);
					continue;

				case "#ENDLOOP":
					if (matches.Length != 2)
					{
						DebugConsole.Raise(new ParameterError(temp_source, "ENDLOOP", matches.Length - 1, 1));
						continue;
					}

					string end_loop_name = matches[1];

					if (!loops.TryGetValue(end_loop_name, out var loop_line))
					{
						DebugConsole.Raise(new MissingLoopError(temp_source, end_loop_name));
						return null;
					}

					loops.Remove(end_loop_name);

					if (!ParameterEvaluator.ToNodeTree<int>(temp_source, new_scope.GetCurrentScope(i), "ENDLOOP", $"{end_loop_name} + 1", out var increment_var_equation))
					{
						throw new UnreachableException("How did this break?");
					}

					new_scope.AddObject(temp_source, new Function(FunctionDefs.GetIndex("SetVar"),
												[new FunctionParameter<string>(value:end_loop_name),
												increment_var_equation!], i));

					new_scope.AddObject(temp_source, new Function(FunctionDefs.GetIndex("GOTO"), 
												[new FunctionParameter<uint>(loop_line)], i));
					continue;

				case "#BREAK":
					if (matches.Length != 2)
					{
						DebugConsole.Raise(new ParameterError(temp_source, "BREAK", matches.Length - 1, 1));
						continue;
					}

					string break_loop_name = matches[1].Trim();

					if (!loops.TryGetValue(break_loop_name, out var break_loop_line))
					{
						DebugConsole.Raise(new MissingLoopError(temp_source, break_loop_name));
						return null;
					}

					// set variable to -1, as this will cause the loop to end
					new_scope.AddObject(temp_source, new Function(FunctionDefs.GetIndex("SetVar"),
											[new FunctionParameter<string>(value: break_loop_name),
											new FunctionParameter<int>(-1)], i));

					new_scope.AddObject(temp_source, new Function(FunctionDefs.GetIndex("GOTO"),
											[new FunctionParameter<int>((int)break_loop_line)], i));

					
					continue;

				/*
				"let" and "var" should be either
				"let TYPE NAME" or "var TYPE NAME" to define a variable but not set a value or

				"let TYPE NAME = VALUE" or "var TYPE NAME = VALUE" to define a variable and set it to that VALUE
				*/
				case "let":
					if (matches.Length < 3)
					{
						DebugConsole.Raise(new ParameterError(temp_source, "let", matches.Length - 1, 2));
						return null;
					}

					if (new_scope.IsLocalVariable(matches[1], i))
					{
						DebugConsole.Raise(new VariableAlreadyExistsError(temp_source, matches[1]));
						return null;
					}

					if (matches.Length >= 3)
					{
						var define_parameters = ParameterProcessor.ProcessParameters(temp_source, new_scope.GetCurrentScope(i), "DefineVar", [matches[1], matches[2]], [typeof(VariableType), typeof(object)]);

						if (define_parameters == null)
						{
							continue; // TODO: raise error
						}

						define_parameters.Add(new FunctionParameter<string>());

						VariableManager.DefineVariable(temp_source, new_scope.GetCurrentScope(i), define_parameters);
					}

					if (matches.Length == 3)
					{
						continue;
					}

					if (matches.Length < 5)
					{
						DebugConsole.Raise(new ParameterError(temp_source, "let", matches.Length - 1, 4));
						return null;
					}

					if (matches[3] != "=")
					{
						DebugConsole.Raise(new UnknownFunctionError(temp_source, matches[3], "operator for 'let' must be '='"));
						return null;
					}

					var set_parameters = ParameterProcessor.ProcessParameters(temp_source, new_scope.GetCurrentScope(i), "DefineVar", [matches[2], matches[4..].JoinToString()], [typeof(object), typeof(VariableObject)]);

					if (set_parameters == null)
					{
						continue; // TODO: raise error
					}

					new_scope.AddObject(temp_source, new Function(FunctionDefs.GetIndex("SetVar"),
											set_parameters, i));
					continue;

				case "var":
					if (matches.Length < 3)
					{
						DebugConsole.Raise(new ParameterError(temp_source, "var", matches.Length - 1, 2));
						return null;
					}

					// predefined in preprocess function

					if (matches.Length == 3)
					{
						continue;
					}

					if (matches.Length < 5)
					{
						DebugConsole.Raise(new ParameterError(temp_source, "var", matches.Length - 1, 4));
						return null;
					}

					if (matches[3] != "=")
					{
						DebugConsole.Raise(new UnknownFunctionError(temp_source, matches[3], "operator for 'var' must be '='"));
						return null;
					}

					var set_var_parameters = ParameterProcessor.ProcessParameters(temp_source, new_scope.GetCurrentScope(i), "DefineVar", [matches[2], matches[4..].JoinToString()], [typeof(object), typeof(VariableObject)]);

					if (set_var_parameters == null)
					{
						continue; // TODO: raise error
					}

					new_scope.AddObject(temp_source, new Function(FunctionDefs.GetIndex("SetVar"),
											set_var_parameters, i));
					continue;

				// set should be "set NAME = VALUE"
				case "set":
					if (matches.Length < 4)
					{
						DebugConsole.Raise(new ParameterError(temp_source, "set", matches.Length - 1, 3));
						return null;
					}

					if (!new_scope.ContainsVariable(matches[1], i, out var type))
					{
						DebugConsole.Raise(new UnknownVariableError(temp_source, matches[1]));
						return null;
					}

					string? set_function;
					switch (matches[2])
					{
						case "=":
							set_function = matches[3..].JoinToString();
							break;

						case "+=":
							if (((byte)type & ((byte)VariableType.Int | (byte)VariableType.Float | (byte)VariableType.String)) == 0)
							{
								DebugConsole.Raise(new SetOperatorError(temp_source, "+=", VariableManager.GetVariableTypeName(type)));
								return null;
							}

							if (type == VariableType.String)
							{
								set_function = $"concat({matches[1]}, ({matches[3..].JoinToString()}))";
								break;
							}

							set_function = $"{matches[1]} + ({matches[3..].JoinToString()})";
							break;

						case "-=":
							if (((byte)type & ((byte)VariableType.Int | (byte)VariableType.Float)) == 0)
							{
								DebugConsole.Raise(new SetOperatorError(temp_source, "-=", VariableManager.GetVariableTypeName(type)));
								return null;
							}

							set_function = $"{matches[1]} - ({matches[3..].JoinToString()})";
							break;

						case "/=":
							if (((byte)type & ((byte)VariableType.Int | (byte)VariableType.Float)) == 0)
							{
								DebugConsole.Raise(new SetOperatorError(temp_source, "/=", VariableManager.GetVariableTypeName(type)));
								return null;
							}

							set_function = $"{matches[1]} / ({matches[3..].JoinToString()})";
							break;

						case "*=":
							if (((byte)type & ((byte)VariableType.Int | (byte)VariableType.Float)) == 0)
							{
								DebugConsole.Raise(new SetOperatorError(temp_source, "*=", VariableManager.GetVariableTypeName(type)));
								return null;
							}

							set_function = $"{matches[1]} * ({matches[3..].JoinToString()})";
							break;

						case "^=":
							if (((byte)type & ((byte)VariableType.Int | (byte)VariableType.Float)) == 0)
							{
								DebugConsole.Raise(new SetOperatorError(temp_source, "^=", VariableManager.GetVariableTypeName(type)));
								return null;
							}

							set_function = $"{matches[1]} ^ ({matches[3..].JoinToString()})";
							break;

						case "%=":
							if (((byte)type & ((byte)VariableType.Int | (byte)VariableType.Float)) == 0)
							{
								DebugConsole.Raise(new SetOperatorError(temp_source, "%=", VariableManager.GetVariableTypeName(type)));
								return null;
							}

							set_function = $"{matches[1]} % ({matches[3..].JoinToString()})";
							break;

						default:
							DebugConsole.Raise(new UnknownFunctionError(temp_source, matches[2]));
							return null;
					}

					var parameters = ParameterProcessor.ProcessParameters(temp_source, new_scope.GetCurrentScope(i), "SetVar", [matches[1], set_function], [typeof(object), typeof(VariableObject)]);

					if (parameters == null)
					{
						return null;
					}

					new_scope.AddObject(temp_source, new Function(FunctionDefs.GetIndex("SetVar"),
											parameters, i));
					continue;

				default:
					break;
			}

			ushort funcIndex = FunctionDefs.GetIndex(first_value);

			if (funcIndex == 0) // null function
			{
				DebugConsole.Raise(new UnknownFunctionError(temp_source, first_value));
				return null;
			}

			if (matches.Length < 3)
			{
				DebugConsole.Raise(new MissingParenthesisError(temp_source, matches.JoinToString()));
				return null;
			}

			if (matches.Length == 3 && matches[1] == "(" && matches[2] == ")")
			{
				new_scope.AddObject(temp_source, new Function(funcIndex, [], i));
				continue;
			}

			if (matches[1] != "(")
			{
				DebugConsole.Raise(new MissingParenthesisError(temp_source, matches.JoinToString()));
				return null;
			}

			var string_parameters = ParseFunctionParameters(temp_source, matches);

			if (string_parameters == null)
			{
				return null;
			}

			FunctionDef functionDef = FunctionDefs.Get(funcIndex);

			if (string_parameters.Count != functionDef.parameters!.Length)
			{
				DebugConsole.Raise(new ParameterError(new Source(i, first_value, obj), functionDef.name!, string_parameters.Count, functionDef.parameters.Length, "Did you forget comma separators??"));
				return null;
			}
			
			List<object>? args = ParameterProcessor.ProcessParameters(temp_source, new_scope.GetCurrentScope(i), functionDef.name!, string_parameters, functionDef.parameters);

			if (args == null)
			{
				return null;
			}

			new_scope.AddObject(temp_source, new Function(funcIndex, args, i));
		}

		return new_scope;
	}

	private static List<string>? ParseFunctionParameters(Source source, string[] matches)
	{
		List<string> res = [];

		List<string> current_parameter = [];

		int parentheses_depth = 0;
		bool in_brackets = false;

		for (int j = 2; j < matches.Length; j++)
		{
			var match = matches[j];

			if (match.StartsWith("//") || (j == matches.Length - 1 && (in_brackets || parentheses_depth > 0 || match != ")")))
			{
				DebugConsole.Raise(new MissingParenthesisError(source, matches.JoinToString()));
				return null;
			}

			if (parentheses_depth == 0 && !in_brackets && (match == "," || match == ")"))
			{
				res.Add(current_parameter.JoinToString());
				current_parameter.Clear();

				if (match == ")")
				{
					break;
				}

				continue;
			}

			switch (match)
			{
				case "[":
					in_brackets = true;
					break;

				case "]":
					if (!in_brackets)
					{
						DebugConsole.Raise(new MissingBracketsError(source, matches.JoinToString()));
						return null;
					}
					in_brackets = false;
					break;

				case "(":
					parentheses_depth += 1;
					break;

				case ")":
					parentheses_depth -= 1;

					if (parentheses_depth < 0)
					{
						throw new UnreachableException();
					}
					break;

				default:
					break;
			}

			current_parameter.Add(match);
		}

		return res;
	}

	private static uint? Run(Scope scope, ScopeVariables? variables, string obj, Source source, bool sync, bool is_scene, uint? gotoLine)
	{
		uint? goto_line = gotoLine;

		var current_variables = scope.CopyVariables(variables);

		for (int i = 0; i < scope.Objects.Count; i++)
		{
			var script_object = scope.Objects[i];

			if (script_object.IsScope)
			{
				if (script_object is not Scope new_scope)
				{
					throw new UnreachableException();
				}

				goto_line = Run(new_scope, current_variables, obj, source, sync, is_scene, goto_line);
			}

			else
			{
				if (script_object is not Function func)
				{
					throw new UnreachableException();
				}
				
				goto_line = RunFunct(current_variables, func, new Source(func.Line, FunctionDefs.Get(func.functionIndex).name, obj, source));
			}

			if (!goto_line.HasValue || goto_line.Value == script_object.Line)
			{
				continue;
			}

			if (goto_line.Value > script_object.Line)
			{
				while (i < scope.Objects.Count && scope.Objects[i].Line < goto_line)
				{
					i++;
				}

				if (i >= scope.Objects.Count)
				{
					return goto_line;
				}
			}
			else
			{
				// goto_line must be less than the current line
				while (i >= 0 && scope.Objects[i].Line > goto_line)
				{
					i--;
				}

				if (i < 0)
				{
					return goto_line;
				}
			}
	
			if (!scope.Objects[i].IsScope)
			{
				goto_line = null;
			}

			i--;
		}

		return null;
	}

	public static void RunScope(Scope scope, string obj, Source source, bool sync = false, bool is_scene = false)
	{
		if (scope.Objects.Count == 0)
		{
			return;
		}

		if (sync)
		{
			Run(scope, Game1.GlobalScope.CopyVariables(null), obj, source, true, is_scene, null);
			return;
		}
		
		Thread t = new Thread(() =>
		{
			if (is_scene)
			{
				Game1.sceneManager.Active = true;
			}

			Run(scope, Game1.GlobalScope.CopyVariables(null), obj, source, false, is_scene, null);

			if (is_scene)
			{
				Game1.sceneManager.Active = false;
			}
		});
		t.IsBackground = true;
		t.Start();
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

				case >=0b1000_0000:
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

	[GeneratedRegex(@"(//.*)| # comments
						("".*?((\\\\"")|([^\\]"")))| # strings
						(\#[A-Za-z0-9_]+)| # tags
						[A-Za-z0-9_]+| # operands
						([!+=<>/*-]=)| # equivalence operators
						(&&|\|\|)| # boolean operators
						[^\s] # misc", 
					RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace)]
	private static partial Regex SplitRegex();
}

