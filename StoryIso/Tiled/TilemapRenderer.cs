using System.Collections.Generic;
using System.Linq;
using DotTiled;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace StoryIso.Tiled;

public class TilemapRenderer
{
	readonly Dictionary<string, Texture2D> _tilesetTextures = new();

	public TilemapRenderer()
	{
		
	}

	public void AddTileset(string name, Texture2D texture)
	{
		_tilesetTextures.TryAdd(name, texture);
	}

	public void ClearTilesets()
	{
		_tilesetTextures.Clear();
	}

	public void Update(GameTime gameTime)
	{
		
	}

	public void Draw(SpriteBatch spriteBatch, Map map, BoundingRectangle boundingRectangle, float layer_depth)
	{
		RectangleF bounding_rect = new RectangleF(boundingRectangle.Center - boundingRectangle.HalfExtents, boundingRectangle.HalfExtents * 2);

		foreach (var layer in map.Layers.OfType<TileLayer>())
		{
			if (layer == null)
			{
				continue;
			}
			
			DrawLayer(spriteBatch, map, layer, bounding_rect, layer_depth);
		}
 	}

	private void DrawLayer(SpriteBatch spriteBatch, Map map, TileLayer layer, RectangleF bounding_rect, float layer_depth)
	{
		if (!layer.Visible)
		{
			return;
		}

		for (int y = 0; y < layer.Height; y++)
		{
			if (layer.OffsetY + y * map.TileHeight > bounding_rect.Bottom) 
			{
				break;
			}

			for (int x = 0; x < layer.Width; x++)
			{
				var tile_rect = new RectangleF(layer.OffsetX + x * map.TileWidth, layer.OffsetY + y * map.TileHeight, map.TileWidth, map.TileHeight);

				// check if the tile is outside the view area
				if (tile_rect.Bottom < bounding_rect.Top ||
					tile_rect.Right < bounding_rect.Left) 
				{
					continue;
				}

				if (tile_rect.Left > bounding_rect.Right)
				{
					break;
				}

				var tile_gid = layer.GetGlobalTileIDAtCoord(x, y);

				if (tile_gid == 0)
				{
					continue;
				}

				var tileset = map.ResolveTilesetForGlobalTileID(tile_gid, out var local_tileID);
				var source_rect = tileset.GetSourceRectangleForLocalTileID(local_tileID);

				var sourceRectangle = new Rectangle(source_rect.X + 1, source_rect.Y + 1, source_rect.Width - 2, source_rect.Height - 2);

				spriteBatch.Draw(_tilesetTextures[tileset.Image.Value.Source.Value],
								tile_rect.TopLeft,
								sourceRectangle,
								Color.White,
								0f,
								Vector2.Zero,
								Vector2.One * (16f / 14f), // shrink to avoid seams
								SpriteEffects.None,
								layer_depth);

			}
		}
	}
}