namespace Entropy.Debugging;

public class UnknownLayerError : IError
{
	readonly string function;
	readonly string given;
	public Source? source { get; set; }
	public string? message { get; set; }

	public UnknownLayerError(Source source, string function, string given, string? message = null)
	{
		this.source = source;
		this.function = function;
		this.given = given;
		this.message = message;
	}

	public string GetMessage()
	{
		return $"UnknownLayerError: Unknown layer given to function {function}. Layer '{given}' does not exist. {message}({source!.Format()})";
	}
}