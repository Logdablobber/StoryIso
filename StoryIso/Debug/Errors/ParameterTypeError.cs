namespace StoryIso.Debugging;

public class ParameterTypeError : IError
{
	readonly string function;
	readonly string given;
	readonly string needed_type;
	public Source? source { get; set; }
	public string? message { get; set; }

	public ParameterTypeError(Source source, string function, string given, string needed, string? message = null)
	{
		this.source = source;
		this.function = function;
		this.given = given;
		this.needed_type = needed;
		this.message = message;
	}

	public string GetMessage()
	{
		return $"ParameterTypeError: Incorrect parameter type given to {function}. {given} was given, which could not be parsed to {needed_type}. {message}({source!.Format()})";
	}
}