using Avalonia;
using Avalonia.Media;
using EntropyEditor.Models;

namespace EntropyEditor.ViewModels;

public class TilemapObjectViewModel : RectangleViewModel
{
	private uint _id;

	public uint Id
	{
		get => _id;
		set => SetProperty(ref _id, value);
	}

	public override string ToString()
	{
		return "Object";
	}

	public TilemapObjectViewModel(RefPoint offset, uint id, float x, float y, float width, float height, int zIndex = 0, Color? color = null, Color? borderColor = null, float opacity = 1f, float borderThickness = 1) 
		: base(offset, x, y, width, height, zIndex, color, borderColor, opacity, borderThickness)
	{
		Id = id;
	}
}
