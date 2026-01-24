using System.Runtime.CompilerServices;

namespace StoryIso.Debugging;

public class WrongVariableTypeError : IError
{
	readonly string variable;
	readonly string given;
	readonly string type;
	public Source source { get; set; }
	public string message { get; set; }

	public WrongVariableTypeError(Source source, string variable, string given, string type, string message = null)
	{
		this.source = source;
		this.variable = variable;
		this.given = given;
		this.type = type;
		this.message = message;
	}

	public string GetMessage()
	{
		return $"WrongVariableTypeError: Value {given} could not be parsed to type '{type}' for variable '{variable}'. {message}({source.Format()})";
	}
}