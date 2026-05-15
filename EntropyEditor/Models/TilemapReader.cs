using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Platform;
using DotTiled;
using DotTiled.Serialization;
using EntropyEditor.ViewModels;

namespace EntropyEditor.Models;

public static class TilemapReader
{
	private static Loader _loader = Loader.Default();

	private static CustomClassDefinition interactionDef = new CustomClassDefinition
	{
		Name = "Interaction",
		UseAs = CustomClassUseAs.Object,
		Members = [
			new BoolProperty { Name = "collides", Value = true },
			new BoolProperty { Name = "defaultToggleState", Value = false },
			new StringProperty { Name = "onInteract", Value = null },
			new StringProperty { Name = "onToggleOn", Value = null },
			new StringProperty { Name = "onToggleOff", Value = null },
			new StringProperty { Name = "onUninteract", Value = null },
			new StringProperty { Name = "whileInteract", Value = null },
		]
	};

	private static CustomClassDefinition triggerDef = new CustomClassDefinition
	{
		Name = "Trigger",
		UseAs = CustomClassUseAs.Object,
		Members = [
			new ColorProperty { Name = "color", Value = Optional.Empty },
			new StringProperty { Name = "onEnter", Value = null },
			new StringProperty { Name = "onExit", Value = null },
			new StringProperty { Name = "onStay", Value = null },
		]
	};

	private static Map LoadMap(string path)
	{
		using var resource = AssetLoader.Open(new Uri(path));
		using var reader = new StreamReader(resource);
		var contents = reader.ReadToEnd();

		using var mapReader = new MapReader(contents, ResolveTileset, ResolveTemplate, ResolveCustomType);

		return mapReader.ReadMap();
	}

	public static readonly string basePath = "avares://EntropyEditor/Assets/Tilemaps/";

	static DotTiled.Tileset ResolveTileset(string source)
	{
		using var resource = AssetLoader.Open(new Uri(Path.Join(basePath, source.TrimStart("../"))));
		using var reader = new StreamReader(resource);
		var contents = reader.ReadToEnd();
		using var tilesetReader = new TilesetReader(contents, ResolveTileset, ResolveTemplate, ResolveCustomType);
		return tilesetReader.ReadTileset();
	}

	static Template ResolveTemplate(string source)
	{
		using var resource = AssetLoader.Open(new Uri(Path.Join(basePath, source.TrimStart("../"))));
		using var reader = new StreamReader(resource);
		var contents = reader.ReadToEnd();
		using var templateReader = new TemplateReader(contents, ResolveTileset, ResolveTemplate, ResolveCustomType);
		return templateReader.ReadTemplate();
	}

	static Optional<ICustomTypeDefinition> ResolveCustomType(string name)
	{
		var allDefinedTypes = new ICustomTypeDefinition[] { interactionDef, triggerDef };

		ICustomTypeDefinition? customType = allDefinedTypes.FirstOrDefault(type => type?.Name == name, defaultValue: null);

		return customType != null ? new Optional<ICustomTypeDefinition>(customType) : Optional.Empty;
	}

	public static TilemapDataViewModel LoadFromFile(string path, out BackgroundMusicData backgroundMusic, out Dictionary<string, int> layers)
	{
		var map = LoadMap(path);
		
		var new_tilemap = new TilemapDataViewModel(new RefPoint(0, 0), map.Width);

		backgroundMusic = new BackgroundMusicData()
		{
			Name = map.TryGetProperty<StringProperty>("bgm", out var music) ? music.Value : "",
			Volume = map.TryGetProperty<FloatProperty>("bgmVolume", out var volume) ? volume.Value : 1f
		};

		// initialize layers with their z-indices based on their order in the file

		layers = new Dictionary<string, int>();

		for (int i = 0; i < map.Layers.Count; i++)
		{
			layers[map.Layers[i].Name] = i;
		}

		foreach (var tileset in map.Tilesets)
		{
			TilesetManager.AddTileset(tileset.Name, tileset);
		}

		foreach (var layer in map.Layers.OfType<TileLayer>())
		{
			for (int y = 0; y < layer.Height; y++)
			{
				for (int x = 0; x < layer.Width; x++)
				{
					var tile_guid = layer.GetGlobalTileIDAtCoord(x, y);

					if (tile_guid == 0)
					{
						continue;
					}

					var tileset = map.ResolveTilesetForGlobalTileID(tile_guid, out var localTileID);

					var tile_pos = new TilePosition() { ZIndex = layers[layer.Name], Position = new BasicPoint(x, y) };

					// supress updates cuz it should only re-mesh after adding all the tiles
					new_tilemap[tile_pos, true] = new TileViewModel(tile_pos, localTileID, tileset.Name, new_tilemap.Offset);
				}
			}
		}

		new_tilemap.GreedyMeshTiles();

		foreach (var layer in map.Layers.OfType<ObjectLayer>())
		{
			foreach (var obj in layer.Objects)
			{
				var new_obj = new TilemapObjectViewModel(offset: new_tilemap.Offset,
														id: obj.ID.GetValueOr(0),
														x: obj.X,
														y: obj.Y,
														width: obj.Width,
														height: obj.Height,
														zIndex: layers[layer.Name],
														color: obj.TryGetProperty<ColorProperty>("color", out var color) ? color.Value.Value.ToAvaloniaColor() : null,
														borderColor: obj.TryGetProperty<ColorProperty>("borderColor", out var borderColor) ? borderColor.Value.Value.ToAvaloniaColor() : null,
														opacity: 0.3f,
														borderThickness: obj.TryGetProperty<FloatProperty>("borderThickness", out var borderThickness) ? borderThickness.Value : 2f);

				// TODO: handle properties for interactions and triggers

				new_tilemap.Add(new_obj);
			}
		}

		return new_tilemap;
	}
}
