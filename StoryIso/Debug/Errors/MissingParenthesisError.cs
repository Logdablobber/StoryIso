namespace StoryIso.Debugging;

public class MissingParenthesisError : IError
{
	public string given;
	public Source? source { get; set; }
	public string? message { get; set; }

	public MissingParenthesisError(Source source, string given, string? message = null)
	{
		this.source = source;
		this.given = given;
		this.message = message;
	}

	public string GetMessage()
	{
		return $"MissingParenthesisError: Missing parenthesis in '{given}'. {message}({source!.Format()})";
	}
}