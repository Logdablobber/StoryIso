namespace Entropy.Debugging;

public class NestedIfError : IError
{
	public Source? source { get; set; }
	public string? message { get; set; }

	public NestedIfError(Source source, string? message = null)
	{
		this.source = source;
		this.message = message;
	}

	public string GetMessage()
	{
		return $"NestedIfError: Cannot nest IF statements. {message}({source!.Format()})";
	}
}