using System.Text.Json.Serialization;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Serialization.Json;

namespace Entropy.Scenes;

public struct CharacterData
{
	public required string name { get; set; }
	public required string animation { get; set; }
	public required string room { get; set; }
	public bool visible { get; set; }

	[JsonConverter(typeof(ColorJsonConverter))]
	public Color? color { get; set; }
}