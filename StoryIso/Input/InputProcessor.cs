using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StoryIso.Debugging;
using StoryIso.Enums;
using StoryIso.Scenes;

namespace StoryIso.Input;

public class InputProcessor
{
    public Dictionary<KeyEvent, string[]> Keybinds = [];

    private KeyboardState? _previousKeystate;

    public void Process(KeyboardState keystate)
    {
	    var source = new Source(0, null, "KeybindManager");
        
	    foreach (var (keyEvent, scenes) in Keybinds)
	    {
		    if (!CheckRunKeybind(keystate, keyEvent))
		    {
			    continue; 
		    }
            
		    foreach (var scene in scenes)
		    {
			    Game1.sceneManager.RunScene(scene, source, keyEvent.Interaction == KeyInteraction.Hold);
		    }
	    }

        _previousKeystate = keystate;
    }
    
    public void AddKeybind(KeyEvent key, string scene)
	{
		if (Keybinds.TryGetValue(key, out var scenes))
		{
			Keybinds[key] = scenes.Union([scene]).ToArray();
			return;
		}

		Keybinds[key] = [scene];
	}
    
	private bool CheckRunKeybind(KeyboardState keystate, KeyEvent keyEvent)
	{
		switch (keyEvent.Interaction)
		{
			case KeyInteraction.Down:
				if (!keystate.IsKeyDown(keyEvent.Key) ||
				    (_previousKeystate != null && !_previousKeystate.Value.IsKeyUp(keyEvent.Key)))
				{
					return false;
				}

				return true;

			case KeyInteraction.Up:
				if (!keystate.IsKeyUp(keyEvent.Key) ||
				    (_previousKeystate != null && !_previousKeystate.Value.IsKeyDown(keyEvent.Key)))
				{
					return false;
				}

				return true;

			case KeyInteraction.Hold:
				if (keystate.IsKeyUp(keyEvent.Key))
				{
					return false;
				}

				return true;

			default:
				throw new NotImplementedException();
		}
	}
}