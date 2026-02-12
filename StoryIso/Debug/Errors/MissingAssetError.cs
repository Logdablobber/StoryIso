namespace StoryIso.Debugging;

public class MissingAssetError : IError
{
	public string given;
	public Source? source { get; set; }
	public string? message { get; set; }

	public MissingAssetError(Source source, string given, string? message = null)
	{
	 	this.source = source;
		this.given = given;
		this.message = message;
	}

	public string GetMessage()
	{
		return $"Missing Asset Error: Unknown asset given. Asset '{given}' does not exist. {message}({source!.Format()})";
	}
}