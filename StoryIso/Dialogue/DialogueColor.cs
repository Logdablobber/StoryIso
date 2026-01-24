using Microsoft.Xna.Framework;

namespace StoryIso.Dialogue;

public struct DialogueColor
{
	public byte[] rgb;

	public Color Get()
	{
		return new Color(rgb[0], rgb[1], rgb[2]);
	}
}