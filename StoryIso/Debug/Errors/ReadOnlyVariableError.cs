namespace StoryIso.Debugging;

public class ReadOnlyVariableError : IError
{
	readonly string given;
	public Source? source { get; set; }
	public string? message { get; set; }

	public ReadOnlyVariableError(Source source, string given, string? message = null)
	{
		this.source = source;
		this.given = given;
		this.message = message;
	}

	public string GetMessage()
	{
		return $"ReadOnlyVariableError: Variable '{given}' is readonly. {message}({source!.Format()})";
	}
}