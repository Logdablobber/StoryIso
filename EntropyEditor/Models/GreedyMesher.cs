using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Avalonia.Controls.Shapes;
using DotTiled;

namespace EntropyEditor.Models;

public static class GreedyMesher
{
	// uses the binary greedy meshing algorithm
	// I can't be fucked to find a link
	public static List<BasicRect> Mesh(List<BitArray> tiles)
	{
		List<BasicRect> rects = [];

		for (int y = 0; y < tiles.Count; y++)
		{
			var row = tiles[y];

			if (!row.HasAnySet())
			{
				continue;
			}

			var left_endpoints = new BitArray(row);

			left_endpoints.LeftShift(1);
			left_endpoints.Not();
			left_endpoints.And(row);

			var right_endpoints = new BitArray(row);

			right_endpoints.RightShift(1);
			right_endpoints.Not();
			right_endpoints.And(row);

			int start_x = -1;

			for (int x = 0; x < row.Count; x++)
			{
				if (left_endpoints[x])
				{
					start_x = x;
				}

				if (start_x != -1 && right_endpoints[x])
				{
					var new_rect = new BasicRect(start_x, y, x - start_x + 1, 1);

					var rect_box = new BitArray(row.Count, false);
					for (int x1 = start_x; x1 <= x; x1++)
					{
						rect_box[x1] = true;
					}

					for (int y1 = y + 1; y1 < tiles.Count; y1++)
					{
						var temp_row = new BitArray(tiles[y1]);

						if (temp_row.And(rect_box).Xor(rect_box).HasAnySet()) // check if you can extend the box
						{
							break;
						}

						tiles[y1].Xor(rect_box); // remove items from next row
						new_rect.IncreaseHeight(1);
					}

					rects.Add(new_rect);
					start_x = -1;
				}
			}
		}

		return rects;
	}
}
