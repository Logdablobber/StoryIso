namespace StoryIso.Debugging;

public class ParameterError : IError
{
	readonly string function;
	readonly int given;
	readonly int needed;
	public Source? source { get; set; }
	public string? message { get; set; }

	public ParameterError(Source source, string function, int given, int needed, string? message = null)
	{
		this.source = source;
		this.function = function;
		this.given = given;
		this.needed = needed;
		this.message = message;
	}

	public string GetMessage()
	{
		return $"ParameterError: Incorrect amount of parameters given to function {function}. {given} were given while {needed} were needed. {message}(at {source!.Format()})";
	}
}