using System;
using System.Collections.Generic;
using StoryIso.Enums;
using StoryIso.Scripting;

namespace StoryIso.Misc;

public static class TypeIndexers 
{
	public const byte INT = 0b00000000;
	public const byte FLOAT = 0b00000001;
	public const byte STRING = 0b00000010;
	public const byte BOOL = 0b00000011;
	public const byte TILE_LAYER_TYPE = 0b00000100;
	public const byte USHORT = 0b00000101;
	public const byte UINT = 0b00000110;
	public const byte BYTE = 0b00000111;
	public const byte RELATIVE_INT = 0b00001000;
	public const byte RELATIVE_FLOAT = 0b00001001;
	public const byte TYPE = 0b00001010;
	public const byte OBJECT = 0b00001011;
	public const byte VARIABLE_OBJECT = 0b00001100;
	public const byte DIRECTION = 0b00001101;

	static readonly Dictionary<Type, byte> typeDict = new Dictionary<Type, byte>()
	{
		{typeof(int), 						INT},
		{typeof(float),						FLOAT},
		{typeof(string), 					STRING},
		{typeof(bool), 						BOOL},
		{typeof(TileLayerType), 			TILE_LAYER_TYPE},
		{typeof(ushort), 					USHORT},
		{typeof(uint), 						UINT},
		{typeof(byte), 						BYTE},
		{typeof(RelativeVariable<int>), 	RELATIVE_INT},
		{typeof(RelativeVariable<float>), 	RELATIVE_FLOAT},
		{typeof(VariableType), 				TYPE},
		{typeof(object), 					OBJECT},
		{typeof(VariableObject), 			VARIABLE_OBJECT},
		{typeof(Direction), 				DIRECTION},
	};

	public static byte GetTypeIndexer(Type type)
	{
		if (typeDict.TryGetValue(type, out var typeIndexer))
		{
			return typeIndexer;
		}

		if (type.IsArray && typeDict.TryGetValue(type.GetElementType()!, out var genericTypeIndexer))
		{
			return (byte)(0b10000000 | genericTypeIndexer);
		}

		throw new NotImplementedException();
	}
}