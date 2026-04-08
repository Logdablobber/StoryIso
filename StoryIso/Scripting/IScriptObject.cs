namespace StoryIso.Scripting;

public interface IScriptObject 
{
	public uint Line { get; }
	public bool IsScope { get; }
}