using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Entropy.Debugging;
using Entropy.Enums;
using Entropy.Misc;
using Entropy.Scripting.Variables;

namespace Entropy.Scripting;

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
		Scope current_scope = new(Game1.GlobalScope, [], start_line, (uint)funcs_string.Length);

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

			var parse_result = ParseKeywords(temp_source,
				current_scope,
				i,
				matches,
				lines,
				ref inside_if,
				ref inside_else,
				loops);
            
			if (parse_result.HasValue)
			{
				if (parse_result.Value)
				{
					continue;
				}

				return null;
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

			if (matches is [_, "(", ")"])
			{
				current_scope.AddObject(temp_source, new Function(funcIndex, [], i));
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

			var functionDef = FunctionDefs.Get(funcIndex);

			if (string_parameters.Count != functionDef.parameters.Length)
			{
				DebugConsole.Raise(new ParameterError(new Source(i, first_value, obj), functionDef.name!, string_parameters.Count, functionDef.parameters.Length, "Did you forget comma separators??"));
				return null;
			}
			
			var args = ParameterProcessor.ProcessParameters(temp_source, current_scope.GetCurrentScope(i), functionDef.name!, string_parameters, functionDef.parameters);

			if (args == null)
			{
				return null;
			}

			current_scope.AddObject(temp_source, new Function(funcIndex, args, i));
		}

		return current_scope;
	}

	private static bool? ParseKeywords(Source source, 
										Scope current_scope,
                                        uint index,
										string[] matches, 
                                        string[][] lines,
										ref bool inside_if,
                                        ref bool inside_else,
										Dictionary<string, uint> loops)
	{
		string first_value = matches[0];
        
		switch (first_value)
		{
			case "#IF":
				inside_if = true;

				string input = matches[1..].JoinToString();

				if (!ParameterEvaluator.ToNodeTree<bool>(source, current_scope.GetCurrentScope(index), "IF", $"!({input})",
					    out var if_condition))
				{
					return true;
				}

				uint? goto_line = null;

				int nesting = 0;

				for (uint j = index + 1; j < lines.Length; j++)
				{
					if (lines[j].Length == 0)
					{
						continue;
					}
                    
					if (lines[j][0] == "#IF")
					{
						nesting += 1;
						continue;
					}

					if (lines[j][0] == "#ENDIF")
					{
						nesting -= 1;
                        
                        if (nesting == -1)
                        {
	                        goto_line = j;
	                        break;
                        }

                        continue;
					}
                    
                    if ((lines[j][0] == "#ELIF" ||
                        lines[j][0] == "#ELSE") && nesting == 0)
                    {
	                    goto_line = j;
	                    break;
                    }

					if (nesting < 0)
					{
						DebugConsole.Raise(new InvalidSceneError(new Source(j, "#IF", source.obj, source), "#ENDIF", "ENDIF requires an if statement in the first place"));
						return false;
					}
				}

				goto_line ??= (uint)lines.Length;

				var if_scope = new Scope(null, [], index, goto_line.Value);

				if_scope.AddObject(source, new Function(FunctionDefs.GOTOIF_Index,
					[
						if_condition!,
						new FunctionParameter<uint>(goto_line.Value)
					],
					index));

				current_scope.AddObject(source, if_scope);
				return true;

			case "#ELIF":
				if (!inside_if || inside_else)
				{
					DebugConsole.Raise(new MissingIfError(source));
					return false;
				}

				var elif_input = matches[1..].JoinToString();

				if (!ParameterEvaluator.ToNodeTree<bool>(source, current_scope.GetCurrentScope(index), "ELIF",
					    $"!({elif_input})", out var elif_condition))
				{
					return true;
				}

				uint? elif_goto_line = null;
				uint? endif_line = null;

				int elif_nesting = 0;

				for (uint j = index + 1; j < lines.Length; j++)
				{
					if (lines[j].Length == 0)
					{
						continue;
					}

					if (lines[j][0] == "#IF")
					{
						elif_nesting += 1;
						continue;
					}
                    
                    if (lines[j][0] == "#ENDIF")
                    {
	                    elif_nesting -= 1;
                        
                        if (elif_nesting == -1)
                        {
	                        endif_line = j;
                            
                            if (!elif_goto_line.HasValue)
                            {
	                            elif_goto_line = j;
                            }
	                        break;
                        }

                        continue;
                    }
                    
					if (!elif_goto_line.HasValue &&
					     (lines[j][0] == "#ELIF" ||
					     lines[j][0] == "#ELSE"))
					{
						elif_goto_line = j;
					}
                    
                    if (elif_nesting < 0)
					{
						DebugConsole.Raise(new InvalidSceneError(new Source(j, "#ELIF", source.obj, source), "#ENDIF",
							"ENDIF requires an if statement in the first place"));
						return false;
					}
				}

				if (!endif_line.HasValue || !elif_goto_line.HasValue)
				{
					endif_line = (uint)lines.Length;

					elif_goto_line ??= endif_line;
				}

				// this is the goto for the IF statement before it
				current_scope.AddObject(source, new Function(FunctionDefs.GetIndex("GOTO"),
					[new FunctionParameter<uint>(endif_line.Value)],
					index - 1));

				var elif_scope = new Scope(null, [], index, elif_goto_line.Value);

				elif_scope.AddObject(source, new Function(FunctionDefs.GOTOIF_Index,
					[
						elif_condition!,
						new FunctionParameter<uint>(elif_goto_line.Value)
					],
					index));

				current_scope.AddObject(source, elif_scope);
				return true;

			case "#ELSE":
				if (!inside_if || inside_else)
				{
					DebugConsole.Raise(new MissingIfError(source));
					return false;
				}

				inside_else = true;

				uint? else_endif_line = null;

				int else_nesting = 0;

				for (uint j = index + 1; j < lines.Length; j++)
				{
                    if (lines[j].Length == 0)
                    {
	                    continue;
                    }
                    
					if (lines[j][0] == "#IF")
					{
						else_nesting += 1;
						continue;
					}

					if (lines[j][0] != "#ENDIF")
					{
						continue;
					}
                    
					else_nesting -= 1;
                        
					if (else_nesting == -1)
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
				current_scope.AddObject(source, new Function(FunctionDefs.GetIndex("GOTO"),
					[new FunctionParameter<uint>(else_endif_line.Value)],
					index - 1));

				current_scope.AddObject(source, new Scope(null, [], index, else_endif_line.Value));
				return true;

			case "#ENDIF":
				inside_if = false;
				inside_else = false;
				return true;

			case "#LOOP": // should be in format #LOOP LOOP_NAME AMOUNT_OF_LOOPS
				if (matches.Length < 3)
				{
					DebugConsole.Raise(new ParameterError(source, "LOOP", matches.Length - 1, 2,
						"LOOP should be given a name and then the amount of cycles to run. Did you forget a comma?"));
					return true;
				}

				string loop_name = matches[1];

				uint end_loop_line = 0;

				for (uint j = index + 1; j < lines.Length; j++)
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

				current_scope.GetCurrentScope(index).DefineVariable(source, loop_variable);

				if (!ParameterEvaluator.ToNodeTree<bool>(source, current_scope.GetCurrentScope(index), "LOOP",
					    $"({loop_name} == {matches[2..].JoinToString()}) || ({loop_name} == -1)", out var condition))
				{
					return true;
				}

				current_scope.AddObject(source, new Function(FunctionDefs.GetIndex("SetVar"),
				[
					new FunctionParameter<string>(value: loop_name),
					new FunctionParameter<int>(0)
				], index - 1));

				var loop_scope = new Scope(null, [], index, end_loop_line);

				loop_scope.AddObject(source, new Function(FunctionDefs.GOTOIF_Index,
				[
					condition!,
					new FunctionParameter<uint>(end_loop_line)
				], index));

				current_scope.AddObject(source, loop_scope);

				loops.Add(loop_name, index);
				return true;

			case "#ENDLOOP":
				if (matches.Length != 2)
				{
					DebugConsole.Raise(new ParameterError(source, "ENDLOOP", matches.Length - 1, 1));
					return true;
				}

				string end_loop_name = matches[1];

				if (!loops.Remove(end_loop_name, out var loop_line))
				{
					DebugConsole.Raise(new MissingLoopError(source, end_loop_name));
					return false;
				}

				if (!ParameterEvaluator.ToNodeTree<int>(source, current_scope.GetCurrentScope(index), "ENDLOOP",
					    $"{end_loop_name} + 1", out var increment_var_equation))
				{
					throw new UnreachableException("How did this break?");
				}

				current_scope.AddObject(source, new Function(FunctionDefs.GetIndex("SetVar"),
				[
					new FunctionParameter<string>(value: end_loop_name),
					increment_var_equation!
				], index));

				current_scope.AddObject(source, new Function(FunctionDefs.GetIndex("GOTO"),
					[new FunctionParameter<uint>(loop_line)], index));
				return true;

			case "#BREAK":
				if (matches.Length != 2)
				{
					DebugConsole.Raise(new ParameterError(source, "BREAK", matches.Length - 1, 1));
					return true;
				}

				string break_loop_name = matches[1].Trim();

				if (!loops.TryGetValue(break_loop_name, out var break_loop_line))
				{
					DebugConsole.Raise(new MissingLoopError(source, break_loop_name));
					return false;
				}

				// set variable to -1, as this will cause the loop to end
				current_scope.AddObject(source, new Function(FunctionDefs.GetIndex("SetVar"),
				[
					new FunctionParameter<string>(value: break_loop_name),
					new FunctionParameter<int>(-1)
				], index));

				current_scope.AddObject(source, new Function(FunctionDefs.GetIndex("GOTO"),
					[new FunctionParameter<int>((int)break_loop_line)], index));


				return true;

			/*
			"let" and "var" should be either
			"let TYPE NAME" or "var TYPE NAME" to define a variable but not set a value or

			"let TYPE NAME = VALUE" or "var TYPE NAME = VALUE" to define a variable and set it to that VALUE
			*/
			case "let":
				if (matches.Length < 3)
				{
					DebugConsole.Raise(new ParameterError(source, "let", matches.Length - 1, 2));
					return false;
				}

				if (current_scope.IsLocalVariable(matches[1], index))
				{
					DebugConsole.Raise(new VariableAlreadyExistsError(source, matches[1]));
					return false;
				}

				if (matches.Length >= 3)
				{
					var define_parameters = ParameterProcessor.ProcessParameters(source,
						current_scope.GetCurrentScope(index), "DefineVar", [matches[1], matches[2]],
						[typeof(VariableType), typeof(object)]);

					if (define_parameters == null)
					{
						return true; // TODO: raise error
					}

					define_parameters.Add(new FunctionParameter<string>());

					VariableManager.DefineVariable(source, current_scope.GetCurrentScope(index), define_parameters);
				}

				if (matches.Length == 3)
				{
					return true;
				}

				if (matches.Length < 5)
				{
					DebugConsole.Raise(new ParameterError(source, "let", matches.Length - 1, 4));
					return false;
				}

				if (matches[3] != "=")
				{
					DebugConsole.Raise(new UnknownFunctionError(source, matches[3],
						"operator for 'let' must be '='"));
					return false;
				}

				var set_parameters = ParameterProcessor.ProcessParameters(source, current_scope.GetCurrentScope(index),
					"DefineVar", [matches[2], matches[4..].JoinToString()], [typeof(object), typeof(VariableObject)]);

				if (set_parameters == null)
				{
					return true; // TODO: raise error
				}

				current_scope.AddObject(source, new Function(FunctionDefs.GetIndex("SetVar"),
					set_parameters, index));
				return true;

			case "var":
				if (matches.Length < 3)
				{
					DebugConsole.Raise(new ParameterError(source, "var", matches.Length - 1, 2));
					return false;
				}

				// predefined in preprocess function

				if (matches.Length == 3)
				{
					return true;
				}

				if (matches.Length < 5)
				{
					DebugConsole.Raise(new ParameterError(source, "var", matches.Length - 1, 4));
					return false;
				}

				if (matches[3] != "=")
				{
					DebugConsole.Raise(new UnknownFunctionError(source, matches[3],
						"operator for 'var' must be '='"));
					return false;
				}

				var set_var_parameters = ParameterProcessor.ProcessParameters(source, current_scope.GetCurrentScope(index),
					"DefineVar", [matches[2], matches[4..].JoinToString()], [typeof(object), typeof(VariableObject)]);

				if (set_var_parameters == null)
				{
					return true; // TODO: raise error
				}

				current_scope.AddObject(source, new Function(FunctionDefs.GetIndex("SetVar"),
					set_var_parameters, index));
				return true;

			// set should be "set NAME = VALUE"
			case "set":
				if (matches.Length < 4)
				{
					DebugConsole.Raise(new ParameterError(source, "set", matches.Length - 1, 3));
					return false;
				}

				if (!current_scope.ContainsVariable(matches[1], index, out var type))
				{
					DebugConsole.Raise(new UnknownVariableError(source, matches[1]));
					return false;
				}

				string? set_function;
				switch (matches[2])
				{
					case "=":
						set_function = matches[3..].JoinToString();
						break;

					case "+=":
						if (((byte)type &
						     ((byte)VariableType.Int | (byte)VariableType.Float | (byte)VariableType.String)) == 0)
						{
							DebugConsole.Raise(new SetOperatorError(source, "+=",
								VariableManager.GetVariableTypeName(type)));
							return false;
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
							DebugConsole.Raise(new SetOperatorError(source, "-=",
								VariableManager.GetVariableTypeName(type)));
							return false;
						}

						set_function = $"{matches[1]} - ({matches[3..].JoinToString()})";
						break;

					case "/=":
						if (((byte)type & ((byte)VariableType.Int | (byte)VariableType.Float)) == 0)
						{
							DebugConsole.Raise(new SetOperatorError(source, "/=",
								VariableManager.GetVariableTypeName(type)));
							return false;
						}

						set_function = $"{matches[1]} / ({matches[3..].JoinToString()})";
						break;

					case "*=":
						if (((byte)type & ((byte)VariableType.Int | (byte)VariableType.Float)) == 0)
						{
							DebugConsole.Raise(new SetOperatorError(source, "*=",
								VariableManager.GetVariableTypeName(type)));
							return false;
						}

						set_function = $"{matches[1]} * ({matches[3..].JoinToString()})";
						break;

					case "^=":
						if (((byte)type & ((byte)VariableType.Int | (byte)VariableType.Float)) == 0)
						{
							DebugConsole.Raise(new SetOperatorError(source, "^=",
								VariableManager.GetVariableTypeName(type)));
							return false;
						}

						set_function = $"{matches[1]} ^ ({matches[3..].JoinToString()})";
						break;

					case "%=":
						if (((byte)type & ((byte)VariableType.Int | (byte)VariableType.Float)) == 0)
						{
							DebugConsole.Raise(new SetOperatorError(source, "%=",
								VariableManager.GetVariableTypeName(type)));
							return false;
						}

						set_function = $"{matches[1]} % ({matches[3..].JoinToString()})";
						break;

					default:
						DebugConsole.Raise(new UnknownFunctionError(source, matches[2]));
						return false;
				}

				var parameters = ParameterProcessor.ProcessParameters(source, current_scope.GetCurrentScope(index),
					"SetVar", [matches[1], set_function], [typeof(object), typeof(VariableObject)]);

				if (parameters == null)
				{
					return false;
				}

				current_scope.AddObject(source, new Function(FunctionDefs.GetIndex("SetVar"),
					parameters, index));
				return true;
            
            case "#RETURN":
				current_scope.AddObject(source, new Function(FunctionDefs.GetIndex("GOTO"),
					[new FunctionParameter<uint>((uint)lines.Length)], index));
				return true;

			default:
				break;
		}

		return null;
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

