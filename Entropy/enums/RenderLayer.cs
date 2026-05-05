using System.Runtime.CompilerServices;

namespace Entropy.Enums;

public enum RenderLayer
{
	None = -1,
	World = 0,
	Background = 1,
	Foreground = 2,
	Characters = 3,
	Player = 4,
	ForeForeground = 5,
	UI = 6
}

public static class RenderLayerExtensions 
{
	public static float GetLayerDepth(this RenderLayer layer)
	{
		return ((int)layer) / 100f;
	}
}