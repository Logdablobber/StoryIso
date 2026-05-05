
using Microsoft.Xna.Framework.Graphics;

namespace Entropy.FileLoading;

public static class FileLoadingManager 
{
	public static void LoadAll(GraphicsDevice graphicsDevice)
	{
		AsepriteLoader.LoadAsefiles(graphicsDevice, "./Content/Aseprite/");
		TextureLoader.LoadTextures(graphicsDevice, "./Content/Textures/");
		FontLoader.LoadFonts(graphicsDevice, "./Content/Fonts/");
		AudioLoader.LoadSounds("./Content/Audio/");
	}
}