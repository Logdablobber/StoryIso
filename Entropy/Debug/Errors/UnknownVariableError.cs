namespace Entropy.Debugging;

public class UnknownVariableError : IError
{
	readonly string given;
	public Source? source { get; set; }
	public string? message { get; set; }

	public UnknownVariableError(Source source, string given, string? message = null)
	{
		this.source = source;
		this.given = given;
		this.message = message;
	}

	public string GetMessage()
	{
		return $"UnknownVariableError: Unknown variable '{given}'. {message}({source!.Format()})";
	}
}