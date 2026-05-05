using System;
using BCnEncoder.Shared.ImageFiles;
using Microsoft.Xna.Framework;

namespace Entropy.Debugging;

public class DebugLine
{
	public DateTime creationTime;
	public string text;
	public Color color;
	public bool visible;
	public int stackCount;

	public DebugLine(string text,
					Color color,
					int stack_count = 1)
	{
		creationTime = DateTime.Now;
		this.text = text;
		this.color = color;
		visible = true;
		stackCount = stack_count;
	}

	public bool CheckIncrementCount(string text, Color color)
	{
		if (text == this.text && color == this.color)
		{
			IncrementCount();
			return true;
		}

		return false;
	}

	public void IncrementCount()
	{
		creationTime = DateTime.Now;
		stackCount += 1;
	}

	public bool TryGet(out string? line)
	{
		if (!visible)
		{
			line = null;
			return false;
		}

		line = text;

		if (stackCount > 1)
		{
			line = $"({stackCount}) {line}";
		}

		return true;
	}
}