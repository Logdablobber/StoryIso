namespace StoryIso.Debugging;

public class InvalidSceneError : IError
{
	public string given;
	public Source source { get; set; }
	public string message { get; set; }

	public InvalidSceneError(Source source, string given, string message = null)
	{
		this.source = source;
		this.given = given;
		this.message = message;
	}

	public string GetMessage()
	{
		return $"InvalidSceneError: Invalid scene '{given}'. {message}({source.Format()})";
	}
}