namespace StoryIso.Debugging;

public class BadVariableNameError : IError
{
	readonly string given;
	public Source? source { get; set; }
	public string? message { get; set; }

	public BadVariableNameError(Source source, string given, string? message = null)
	{
		this.source = source;
		this.given = given;
		this.message = message;
	}

	public string GetMessage()
	{
		return $"BadVariableNameError: '{given}' is not a usable name for a variable. {message}({source!.Format()})";
	}
}