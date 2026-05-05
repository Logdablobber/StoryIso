using System;

namespace Entropy.Misc;

public static class OptionalExtensions
{
	public static Optional<T> ToOptional<T>(this IOptional value)
	{
        if (!value.HasValue)
        {
	        return new Optional<T>();
        }
        
		if (typeof(T) != value.ValueType)
		{
			throw new InvalidCastException($"Type of {value.ValueType.Name} cannot be converted to {typeof(T).Name}");
		}

		return (Optional<T>)value;
	}
}