using System.Runtime.CompilerServices;

namespace StoryIso.Debugging;

public class VariableAlreadyExistsError : IError
{
	readonly string given;
	public Source? source { get; set; }
	public string? message { get; set; }

	public VariableAlreadyExistsError(Source source, string given, string? message = null)
	{
		this.source = source;
		this.given = given;
		this.message = message;
	}

	public string GetMessage()
	{
		return $"VariableAlreadyExistsError: Variable '{given}' is already defined but was defined again. {message}({source!.Format()})";
	}
}