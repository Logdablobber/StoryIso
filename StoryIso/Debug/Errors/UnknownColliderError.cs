namespace StoryIso.Debugging;

public class UnknownColliderError : IError
{
	readonly string function;
	readonly string given;
	public Source source { get; set; }
	public string message { get; set; }

	public UnknownColliderError(Source source, string function, string given, string message = null)
	{
	 	this.source = source;
		this.function = function;
		this.given = given;
		this.message = message;
	}

	public string GetMessage()
	{
		return $"UnknownColliderError: Unknown collider given to function {function}. Collider '{given}' does not exist. {message}({source.Format()})";
	}
}