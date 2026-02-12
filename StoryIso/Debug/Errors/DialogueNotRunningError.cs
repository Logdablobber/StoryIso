namespace StoryIso.Debugging;

public class DialogueNotRunningError : IError
{
	public Source? source { get; set; }
	public string? message { get; set; }

	public DialogueNotRunningError(Source? source = null, string? message = null)
	{
		this.source = source;
		this.message = message;
	}

	public string GetMessage()
	{
		if (source == null)
		{
			return $"DialogueNotRunningError: No dialogue running. {message}";
		}

		return $"DialogueNotRunningError: No dialogue running. {message}({source.Format()})";
	}
}