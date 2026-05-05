namespace Entropy.Debugging;

public class ParameterValueError : IError
{
	readonly string function;
	readonly string given;
	public Source? source { get; set; }
	public string? message { get; set; }

	public ParameterValueError(Source source, string function, string given, string? message = null)
	{
		this.source = source;
		this.function = function;
		this.given = given;
		this.message = message;
	}

	public string GetMessage()
	{
		return $"ParameterValueError: Invalid value of {given} given to function {function}. {message}({source!.Format()})";
	}
}