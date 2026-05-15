using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;
using EntropyEditor.ViewModels;

namespace EntropyEditor.DataTemplates;

public class TilemapTemplateSelector : IDataTemplate
{
	[Content]
	public Dictionary<string, IDataTemplate> AvailableTemplates { get; } = new Dictionary<string, IDataTemplate>();

	public Control? Build(object? param)
	{
		var key = param?.ToString();
		if (key == null)
		{
			throw new ArgumentNullException(nameof(param));
		}

		return AvailableTemplates[key].Build(param);
	}

	public bool Match(object? data)
	{
		var key = data?.ToString();

		return data is ITilemapItem
				&& !string.IsNullOrEmpty(key)
				&& AvailableTemplates.ContainsKey(key);
	}
}
