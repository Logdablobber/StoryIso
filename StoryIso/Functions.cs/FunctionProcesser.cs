using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Xna.Framework;
using StoryIso.Debugging;
using StoryIso.Enums;
using StoryIso.Misc;
using StoryIso.Scenes;

namespace StoryIso.Functions;

public static partial class FunctionProcessor
{
	public static void Initialize()
	{
		FunctionDefs.Initialize();
		OperatorDefs.Initialize();
	}

	private static string[][] ParseLines(string input)
	{
		return (from line 
					in TextFormatter.SplitLines(input)
					select 
					(from match 
					in SplitRegex().Matches(line.Trim())
					where match.Value != ""
					select match.Value).ToArray()).ToArray();
	}

	public static void Preprocess(string obj, string funcs_string, uint start_line = 0)
	{
		var lines = ParseLines(funcs_string);

		for (uint i = start_line; i < lines.Length; i++)
		{
			var matches = lines[i];

			if (matches.Length == 0)
			{
				continue;
			}

			if (matches[0].Trim() != ".DefineVar")
			{
				continue;
			}

			if (matches.Length != 4)
			{
				continue; // TODO: raise error
			}

			var type = VariableManager.GetVariableType(matches[1].Trim());

			if (type == VariableType.None)
			{
				continue; // TODO: raise error
			}

			VariableManager.DefineVariable(type, matches[2].Trim(), null, new Source(i, "DefineVar", obj));
		}
	}

	public static List<Function>? Process(string obj, string funcs_string, uint start_line = 0)
	{
		List<Function> funcs = [];

		var lines = ParseLines(funcs_string);

		bool inside_if = false;
		bool inside_else = false;

		for (uint i = start_line; i < lines.Length; i++)
		{
			var matches = lines[i];

			if (matches.Length == 0)
			{
				continue;
			}

			string first_value = matches[0].Trim();

			if (first_value.StartsWith("//"))
			{
				continue; // comment
			}

			if (first_value.StartsWith('#'))
			{
				switch (first_value)
				{
					case "#IF":

					    if (inside_if)
						{
							DebugConsole.Raise(new NestedIfError(new Source(i, null, obj)));
							return null;
						}

						inside_if = true;

						string input = matches[1].Trim();

						var postfix = ParameterEvaluator.Postfix<bool>(new Source(i, null, obj), "IF", input);

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

						if (postfix == null)
						{
							continue;
						}

						funcs.Add(new Function(FunctionType.GOTOIF, 
													[postfix, 
													new FunctionParameter<uint>(goto_line.Value)], 
												i));
						continue;

					case "#ELIF":
					    if (!inside_if || inside_else)
						{
							DebugConsole.Raise(new MissingIfError(new Source(i, null, obj)));
							return null;
						}

						string elif_input = matches[1].Trim();

						var elif_postfix = ParameterEvaluator.Postfix<bool>(new Source(i, null, obj), "ELIF", elif_input);

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

						if (elif_postfix == null)
						{
							continue;
						}

						// this is the goto for the IF statement before it
						funcs.Add(new Function(FunctionType.GOTO,
												[new FunctionParameter<uint>(endif_line.Value)],
												i - 1));

						funcs.Add(new Function(FunctionType.GOTOIF, 
													[elif_postfix, 
													new FunctionParameter<uint>(elif_goto_line.Value)], 
												i));
						continue;

					case "#ELSE":
						if (!inside_if || inside_else)
						{
							DebugConsole.Raise(new MissingIfError(new Source(i, null, obj)));
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
						funcs.Add(new Function(FunctionType.GOTO,
												[new FunctionParameter<uint>(else_endif_line.Value)],
												i - 1));
						break;
					
					case "#ENDIF":
					    inside_if = false;
						inside_else = true;
						break;

					default:
						break;
				}

				continue;
			}

			//functions start with a '.' for formatting reasons
			if (first_value[0] != '.') 
			{
				DebugConsole.Raise(new UnknownFunctionError(new Source(i, null, obj), first_value));
				return null;
			}

			FunctionType func = FunctionDefs.Get(first_value[1..]).type;

			if (func == FunctionType.None)
			{
				DebugConsole.Raise(new UnknownFunctionError(new Source(i, null, obj), first_value));
				return null;
			}

			if (matches.Length == 1)
			{
				funcs.Add(new Function(func, [], i));
			}

			List<string> string_parameters = [];

			for (int j = 1; j < matches.Length; j++)
			{
				var match = matches[j].Trim();

				if (match.StartsWith("//"))
				{
					break; // this would be a comment
				}

				string_parameters.Add(match);
			}

			FunctionDef functionDef = FunctionDefs.Get(func);

			if (string_parameters.Count != functionDef.parameters!.Length)
			{
				DebugConsole.Raise(new ParameterError(new Source(i, first_value, obj), functionDef.name!, string_parameters.Count, functionDef.parameters.Length));
				return null;
			}
			
			List<object>? args = ParameterProcessor.ProcessParameters(new Source(i, functionDef.name!, obj), functionDef.name!, string_parameters, functionDef.parameters);

			if (args == null)
			{
				return null;
			}

			funcs.Add(new Function(func, args, i));
		}

		return funcs;
	}

	private static void RunFunctsThread(List<Function> funcs, string obj, Source? source, bool is_scene)
	{
		if (is_scene)
		{
			Game1.sceneManager.Active = true;
		}

		for (int i = 0; i < funcs.Count; i++)
		{
			while (Game1.sceneManager.dialogueManager.Active)
			{
				Thread.Sleep(25);
			}
 
			var func = funcs[i];

			uint? goto_line = RunFunct(func, new Source(func.line, FunctionDefs.Get(func.function).name!, obj, source));

			if (!goto_line.HasValue || goto_line.Value == func.line)
			{
				continue;
			}

			if (goto_line.Value > func.line)
			{
				while (i < funcs.Count && funcs[i].line < goto_line)
				{
					i++;
				}

				i--; // because the loop adds one each time
				continue;
			}

			// goto_line must be less than the current line
			while (i >= 0 && funcs[i].line > goto_line)
			{
				i--;
			}
			i--;
		}

		if (is_scene)
		{
			Game1.sceneManager.Active = false;
		}
	}

	public static void RunFuncts(List<Function> funcs, string obj, Source? source = null, bool sync = false, bool is_scene = false)
	{
		if (funcs.Count == 0)
		{
			return;
		}

		if (sync)
		{
			for (int i = 0; i < funcs.Count; i++)
			{
				var func = funcs[i];

				uint? goto_line = RunFunct(func, new Source(func.line, FunctionDefs.Get(func.function).name!, obj, source));

				if (!goto_line.HasValue || goto_line.Value == func.line)
				{
					continue;
				}

				if (goto_line.Value > func.line)
				{
					while (i < funcs.Count && funcs[i].line < goto_line)
					{
						i++;
					}

					i--; // because the loop adds one each time
					continue;
				}

				// goto_line must be less than the current line
				while (i >= 0 && funcs[i].line > goto_line)
				{
					i--;
				}
				i--;
			}

			return;
		}

		Thread t = new Thread(() =>
		{
			RunFunctsThread(funcs, obj, source, is_scene);
		});
		t.IsBackground = true;
		t.Start();
	}

	private static uint? RunFunct(Function func, Source? source)
	{
		FunctionDef functionDef = FunctionDefs.Get(func.function);

		return functionDef.function!(func.parameters, source);
	}

	// splits based on commas
	// and ignores commas inside of brackets or quotes
	// also separates functions (starting with .) and control operators (starting with #)
	// really f**king complicated s**t
	[GeneratedRegex(@"\s*(//.*)|([.#][^\s]+\b)|(([^"",]*([[][^[]+])[^"",]*)|(([^"",]*(""(\\""|[^""])*"")?[^"",]*)+))", RegexOptions.Compiled | RegexOptions.Singleline)]
	private static partial Regex SplitRegex();
}