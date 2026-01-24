using Microsoft.Xna.Framework;

namespace StoryIso.Debugging;

public class DebugLine
{
	public float creationTime;
	public string text;
	public Color color;
	public bool visible;

	public DebugLine(float creation_time,
					string text,
					Color color)
	{
		creationTime = creation_time;
		this.text = text;
		this.color = color;
		visible = true;
	}
}