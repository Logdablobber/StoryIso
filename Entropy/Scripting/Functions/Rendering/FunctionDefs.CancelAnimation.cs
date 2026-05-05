using Entropy.ECS;
using Entropy.Misc;

namespace Entropy.Scripting;

static partial class FunctionDefs
{
    /// <summary>
    /// Cancel Animation
    /// <para> Cancels all animation for the target given as a string.</para>
    /// </summary>
    /// 
    /// <returns></returns>
    private static readonly FunctionDef CancelAnimation = new()
    {
        name = "CancelAnimation",
        parameters =
            [typeof(string)],
        function = (_, args, _) =>
        {
            var target = args[0].ToOptional<string>();

            if (!target.HasValue)
            {
                return null;
            }

            RenderSystem.StopAnimations(target.Value);
            return null;
        }
    };
}