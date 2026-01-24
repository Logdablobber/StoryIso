namespace StoryIso.Debugging;

public class MissingIfError : IError
{
	public Source source { get; set; }
	public string message { get; set; }

	public MissingIfError(Source source, string message = null)
	{
		this.source = source;
		this.message = message;
	}

	public string GetMessage()
	{
		return $"MissingIfError: Cannot run ELSE or ELIF without IF. {message}({source.Format()})";
	}
}