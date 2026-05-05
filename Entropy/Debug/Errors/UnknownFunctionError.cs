namespace Entropy.Debugging;

public class UnknownFunctionError : IError
{
	readonly string given;
	public Source? source { get; set; }
	public string? message { get; set; }

	public UnknownFunctionError(Source source, string given, string? message = null)
	{
		this.source = source;
		this.given = given;
		this.message = message;
	}

	public string GetMessage()
	{
		return $"UnknownFunctionError: Unknown function '{given}'. {message}(at {source!.Format()})";
	}
}