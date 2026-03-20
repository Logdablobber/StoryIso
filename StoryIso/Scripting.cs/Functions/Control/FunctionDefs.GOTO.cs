using StoryIso.Misc;

namespace StoryIso.Scripting;

static partial class FunctionDefs
{
	/// <summary>
	/// GOTO
	/// <para> Goes to a given line.</para>
	/// </summary>
	/// 
	/// <returns></returns>
	static readonly FunctionDef GOTO = new()
	{
		name = "GOTO",
		parameters = [typeof(uint)],
		function = (_, args, source) => 
		{
			Optional<uint> line = ParameterProcessor.Convert<uint>(source, args![0]);

			if (!line.HasValue)
			{
				return null;
			}

			return line.Value;
		}
	};
}