using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Entropy.Entities;

public class ButtonComponent
{
	public readonly string name;
	public readonly SizeF size;

	public string? onLeftClick;
	public string? onLeftRelease;
	public string? whileLeftHeld;
	public string? onRightClick;
	public string? onRightRelease;
	public string? whileRightHeld;
	public string? onEnter;
	public string? onExit;
	public string? onStay;

	public bool left_held = false;
	public bool right_held = false;

	public ButtonComponent(string name, 
							SizeF size, 
							string? on_left_click, 
							string? on_left_release, 
							string? while_left_held,
							string? on_right_click,
							string? on_right_release,
							string? while_right_held,
							string? on_enter,
							string? on_exit,
							string? on_stay)
	{
		this.name = name;
		this.size = size;
		this.onLeftClick = on_left_click;
		this.onLeftRelease = on_left_release;
		this.whileLeftHeld = while_left_held;
		this.onRightClick = on_right_click;
		this.onRightRelease = on_right_release;
		this.whileRightHeld = while_right_held;
		this.onEnter = on_enter;
		this.onExit = on_exit;
		this.onStay = on_stay;
	}
}