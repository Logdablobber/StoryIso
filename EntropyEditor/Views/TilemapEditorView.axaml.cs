using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.ComponentModel;
using EntropyEditor.Models;
using EntropyEditor.ViewModels;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Avalonia.Input;
using DotTiled;

namespace EntropyEditor.Views;

public partial class TilemapEditorView : UserControl
{

	private bool _isDragging = false;
	private Point _dragStart;

	public TilemapEditorView()
	{
		InitializeComponent();
	}

	protected override void OnPointerPressed(PointerPressedEventArgs e)
	{
		base.OnPointerPressed(e);

		if ((e.KeyModifiers & KeyModifiers.Shift) == 0)
		{
			if (DataContext is not TilemapViewModel viewModel)
			{
				return;
			}

			viewModel.Tilemap.SetTile(new TilePosition(viewModel.GetTileAtPoint(e.GetPosition(tilemapCanvas)), 0), 1, "tileset-atlas");
			return;
		}

		e.Pointer.Capture(tilemapCanvas);
		_dragStart = e.GetPosition(tilemapCanvas) - (DataContext as TilemapViewModel)?.Tilemap.Offset.Value ?? new Point(0, 0);
		_isDragging = true;

		(DataContext as TilemapViewModel)?.IsDragging = true;
	}

	protected override void OnPointerReleased(PointerReleasedEventArgs e)
	{
		base.OnPointerReleased(e);
		e.Pointer.Capture(null); // Release capture
	}

	protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
	{
		base.OnPointerCaptureLost(e);
		Cursor = Cursor.Default;
		_isDragging = false;
		
		(DataContext as TilemapViewModel)?.IsDragging = false;
	}

	protected override void OnPointerMoved(PointerEventArgs e)
	{
		base.OnPointerMoved(e);

		if (!_isDragging || DataContext is not TilemapViewModel viewModel)
		{
			return;
		}
		
		var point = e.GetPosition(tilemapCanvas);

		Cursor = new Cursor(StandardCursorType.SizeAll);

		viewModel.Tilemap.Offset = new RefPoint(point - _dragStart);
	}
}