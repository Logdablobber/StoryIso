using System.Text.Json;

namespace StoryIso.UI;

public static class UIGenerator
{
	public static UIData? GenerateUI(string json)
	{
		return JsonSerializer.Deserialize<UIData>(json, Game1.DeserializeOptions);
	}
}