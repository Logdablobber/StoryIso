namespace StoryIso.Debugging;

public class MissingLoopError : IError
{
	public string given;
	public Source? source { get; set; }
	public string? message { get; set; }

	public MissingLoopError(Source source, string given, string? message = null)
	{
		this.source = source;
		this.given = given;
		this.message = message;
	}

	public string GetMessage()
	{
		return $"MissingLoopError: No loop exists by name '{given}'. {message}({source!.Format()})";
	}
}