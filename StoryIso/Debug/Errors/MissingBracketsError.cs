namespace StoryIso.Debugging;

public class MissingBracketsError : IError
{
	public string given;
	public Source? source { get; set; }
	public string? message { get; set; }

	public MissingBracketsError(Source source, string given, string? message = null)
	{
		this.source = source;
		this.given = given;
		this.message = message;
	}

	public string GetMessage()
	{
		return $"MissingBracketsError: Missing brackets in '{given}'. {message}({source!.Format()})";
	}
}