namespace StoryIso.Debugging;

public class UndefinedVariableError : IError
{
	readonly string name;
	public Source? source { get; set; }
	public string? message { get; set; }

	public UndefinedVariableError(Source source, string name, string? message = null)
	{
	 	this.source = source;
		this.name = name;
		this.message = message;
	}

	public string GetMessage()
	{
		return $"UndefinedVariableError: Variable {name} is not defined. {message}({source!.Format()})";
	}
}