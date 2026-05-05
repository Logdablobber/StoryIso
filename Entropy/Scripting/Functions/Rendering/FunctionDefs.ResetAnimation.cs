using Entropy.ECS;
using Entropy.Enums;
using Entropy.Misc;

namespace Entropy.Scripting;

static partial class FunctionDefs
{
    /// <summary>
    /// Reset Animation
    /// <para> Resets an animation timer for a target, takes in the target and animation name as a string.</para>
    /// </summary>
    /// 
    /// <returns></returns>
    private static readonly FunctionDef ResetAnimation = new()
    {
        name = "ResetAnimation",
        parameters =
            [typeof(string), typeof(AnimationType)],
        function = (_, args, source) =>
        {
            var target = args[0].ToOptional<string>();
            var animation = args[1].ToOptional<AnimationType>();

            if (!target.HasValue || !animation.HasValue )
            {
                return null;
            }

            RenderSystem.ResetAnimation(source, target.Value, animation.Value);
            return null;
        }
    };
}