namespace Entropy.Debugging;

public class ParameterProcessError : IError
{
	public Source? source { get; set; }
	public string? message { get; set; }

	public ParameterProcessError(Source source, string? message = null)
	{
		this.source = source;
		this.message = message;
	}

	public string GetMessage()
	{
		return $"ParameterProcessError: Cannot parse line to given type. {message}({source!.Format()})";
	}
}