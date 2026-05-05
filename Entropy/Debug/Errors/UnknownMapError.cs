namespace Entropy.Debugging;

public class UnknownMapError : IError
{
	readonly string function;
	readonly string given;
	public Source? source { get; set; }
	public string? message { get; set; }

	public UnknownMapError(Source source, string function, string given, string? message = null)
	{
	 	this.source = source;
		this.function = function;
		this.given = given;
		this.message = message;
	}

	public string GetMessage()
	{
		return $"UnknownMapError: Unknown map given to function {function}. Map '{given}' does not exist. {message}({source!.Format()})";
	}
}