using Microsoft.Xna.Framework.Input;
using Entropy.Enums;

namespace Entropy.Input;

public struct KeyEvent
{
    public Keys Key;
    public KeyInteraction Interaction;

    public KeyEvent(Keys key, KeyInteraction interaction)
    {
	    this.Key = key;
        this.Interaction = interaction;
    }
}