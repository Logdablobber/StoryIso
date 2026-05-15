using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Media;
using EntropyEditor.Models;

namespace EntropyEditor.ViewModels;

public interface ITilemapItem : INotifyPropertyChanged, INotifyPropertyChanging
{
	public float X { get; set; }
	public float Y { get; set; }
	public int ZIndex { get; set; }
	public TilePosition Position { get; }

	public void OnOffsetChanged();
}
