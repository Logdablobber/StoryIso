namespace Entropy.Debugging;

public class UnknownTileError : IError
{
	readonly string function;
	readonly int given;
	public Source? source { get; set; }
	public string? message { get; set; }

	public UnknownTileError(Source source, string function, int given, string? message = null)
	{
		this.source = source;
		this.function = function;
		this.given = given;
		this.message = message;
	}

	public string GetMessage()
	{
		return $"UnknownTileError: Unknown tile GUID given to function {function}. GUID {given} is out of range. {message}({source!.Format()})";
	}
}