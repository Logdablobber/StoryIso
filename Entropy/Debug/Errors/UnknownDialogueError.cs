namespace Entropy.Debugging;

public class UnknownDialogueError : IError
{
	readonly string function;
	readonly string given;
	public Source? source { get; set; }
	public string? message { get; set; }

	public UnknownDialogueError(Source source, string function, string given, string? message = null)
	{
		this.source = source;
		this.function = function;
		this.given = given;
		this.message = message;
	}

	public string GetMessage()
	{
		return $"UnknownDialogueError: Unknown dialogue given to function {function}. Dialogue '{given}' does not exist. {message}(at {source!.Format()})";
	}
}