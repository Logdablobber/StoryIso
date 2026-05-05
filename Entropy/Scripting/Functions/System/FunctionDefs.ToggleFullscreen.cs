using Entropy.Misc;

namespace Entropy.Scripting;

static partial class FunctionDefs
{
    /// <summary>
    /// Toggle Fullscreen
    /// <para>Toggles whether or not the game is fullscreen</para>
    /// </summary>
    /// 
    /// <returns></returns>
    private static readonly FunctionDef ToggleFullscreen = new()
    {
        name = "ToggleFullscreen",
        parameters = [],
        function = (_, _, _) =>
        {
            Game1.ToggleFullscreen();
            return null;
        }
    };
}