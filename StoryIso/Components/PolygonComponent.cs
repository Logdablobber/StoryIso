using System.Linq;
using System.Runtime.CompilerServices;
using DotTiled;
using MadWorldNL.EarCut.Logic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Screens;
using MonoGame.Extended.Shapes;
using StoryIso.Misc;

namespace StoryIso.ECS;

public class PolygonComponent
{
	public readonly Vector2[] vertices;
	public readonly int[] indices;
	private readonly BasicEffect effect;

	public PolygonComponent(GraphicsDevice graphics, Vector2[] vertices)
	{
		this.effect = new BasicEffect(graphics)
		{
			VertexColorEnabled = true,
			LightingEnabled = false,
		};

		this.vertices = vertices;

		double[] vertices_array = new double[vertices.Length * 2];
		
		for (int i = 0; i < vertices.Length; i++)
		{
			vertices_array[i * 2] = vertices[i].X;
			vertices_array[i * 2 + 1] = vertices[i].Y;
		}

		indices = EarCut.Tessellate(vertices_array, null, 2).ToArray();
	}

	public void Draw(Color color, Vector2 position, Vector2 scale)
	{
		VertexPositionColor[] vert = new VertexPositionColor[vertices.Length];

		for (int i = 0; i < vertices.Length; i++)
		{
			Vector2 screen_position = new(MiscFuncs.InverseLerp(0, Game1.ScreenWidth, position.X + vertices[i].X * scale.X) * 2 - 1, 
											MiscFuncs.InverseLerp(0, Game1.ScreenHeight, position.Y + vertices[i].Y * scale.Y) * -2 + 1);

			vert[i].Position = new Vector3(screen_position, 0);
			vert[i].Color = color;
		}

		effect.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
		effect.GraphicsDevice.BlendState = BlendState.Opaque;

		foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
		{
			effectPass.Apply();
			effect.GraphicsDevice.DrawUserIndexedPrimitives(
				PrimitiveType.TriangleList, vert, 0, vert.Length, indices, 0, indices.Length / 3);
		}
	}
}