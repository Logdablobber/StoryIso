namespace Entropy.Debugging;

public class SetOperatorError : IError
{
	readonly string oper;
	readonly string given;
	public Source? source { get; set; }
	public string? message { get; set; }

	public SetOperatorError(Source source, string oper, string given, string? message = null)
	{
		this.oper = oper;
		this.source = source;
		this.given = given;
		this.message = message;
	}

	public string GetMessage()
	{
		return $"SetOperatorError: Cannot use operator '{oper}' with type '{given}'. {message}({source!.Format()})";
	}
}