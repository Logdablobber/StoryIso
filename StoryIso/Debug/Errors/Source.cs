namespace StoryIso.Debugging;

public class Source
{
	uint line;
	string? function;
	string obj;
	Source? source;

	public Source(uint line, string? function, string obj, Source? source = null)
	{
		this.line = line;
		this.function = function;
		this.obj = obj;
		this.source = source;
	}

	public string Format()
	{
		if (function != null)
		{
			return $"in function '{function}' at line {line} of {obj}{(source != null ? " " : "")}{source}";
		}
		
		return $"at line {line} of {obj}{(source != null ? " " : "")}{source}";
	}
}