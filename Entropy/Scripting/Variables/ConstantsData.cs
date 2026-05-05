using System.Text.Json.Serialization;

namespace Entropy.Scripting.Variables;

public struct ConstantsData
{
	public IConstantData[] Constants { get; set; }
}

[JsonDerivedType(typeof(StringConstant), typeDiscriminator: "string")]
[JsonDerivedType(typeof(FloatConstant), typeDiscriminator: "float")]
[JsonDerivedType(typeof(IntConstant), typeDiscriminator: "int")]
[JsonDerivedType(typeof(BoolConstant), typeDiscriminator: "bool")]
public interface IConstantData
{
	public string Name { get; set; }
}

public struct StringConstant : IConstantData
{
	public string Name { get; set; }
	public string Value { get; set; }
}

public struct BoolConstant : IConstantData
{
	public string Name { get; set; }
	public bool Value { get; set; }
}

public struct IntConstant : IConstantData
{
	public string Name { get; set; }
	public int Value { get; set; }
}

public struct FloatConstant : IConstantData
{
	public string Name { get; set; }
	public float Value { get; set; }
}