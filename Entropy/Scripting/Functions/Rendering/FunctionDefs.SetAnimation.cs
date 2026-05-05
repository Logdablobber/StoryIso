using Entropy.ECS;
using Entropy.Enums;
using Entropy.Misc;

namespace Entropy.Scripting;

static partial class FunctionDefs
{
    /// <summary>
    /// Set Animation
    /// <para> Sets an animation for a target, takes in the target and animation name as a string and the parameter as a float.</para>
    /// </summary>
    /// 
    /// <returns></returns>
    private static readonly FunctionDef SetAnimation = new()
    {
        name = "SetAnimation",
        parameters =
            [typeof(string), typeof(AnimationType), typeof(float)],
        function = (_, args, source) =>
        {
            var target = args[0].ToOptional<string>();
            var animation = args[1].ToOptional<AnimationType>();
            var parameter = args[2].ToOptional<float>();

            if (!target.HasValue || !animation.HasValue || !parameter.HasValue)
            {
                return null;
            }

            RenderSystem.AddAnimation(source, target.Value, animation.Value, parameter.Value);
            return null;
        }
    };
}