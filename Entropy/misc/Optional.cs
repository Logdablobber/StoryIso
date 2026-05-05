using System;

namespace Entropy.Misc;

public struct Optional<T> : IOptional
{
	public readonly Type ValueType => typeof(T);

	private readonly bool _hasValue;

	public readonly bool HasValue => _hasValue;
	private readonly T value;
	public T Value
	{
		get
		{
			if (HasValue)
			{
				return value;
			}
			
			throw new InvalidOperationException();
		}
	}

	public Optional(T value)
	{
		this.value = value;
		this._hasValue = true;
	}

	public static explicit operator T(Optional<T> optional)
    {
        return optional.Value;
    }
    public static implicit operator Optional<T>(T value)
    {
        return new Optional<T>(value);
    }

    public override bool Equals(object? obj)
    {
        if (obj is Optional<T>)
            return this.Equals((Optional<T>)obj);
        else
            return false;
    }
    public bool Equals(Optional<T> other)
    {
        if (HasValue && other.HasValue)
            return object.Equals(value, other.value);
        else
            return HasValue == other.HasValue;
    }

	public override int GetHashCode() 
	{
		return base.GetHashCode();
	}

	public readonly Type GetUnderlyingType()
	{
		return typeof(T);
	}

	public override string? ToString()
	{
		if (!HasValue)
		{
			return null;
		}

		return value!.ToString();
	}
}