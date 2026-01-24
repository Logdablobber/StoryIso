namespace StoryIso.Debugging;

public interface IError
{
	string message { get; set; }
	Source source { get; set; }

	string GetMessage();
}