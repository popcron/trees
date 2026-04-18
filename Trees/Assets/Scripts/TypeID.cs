using System;

/// <summary>
/// Represents a <see cref="System.Type"/> object.
/// </summary>
[Serializable]
public struct TypeID
{
    public ulong value;

    public Type Type
    {
        get
        {
            if (TypeTable.TryGet(value, out Type type))
            {
                return type;
            }

            return null;
        }
    }

    public TypeID(Type type)
    {
        value = TypeTable.AddOrSet(type);
    }

    public readonly override string ToString()
    {
        if (TypeTable.TryGet(value, out Type type))
        {
            return type.ToString();
        }

        return value.ToString();
    }

    public static TypeID Get<T>()
    {
        return Cache<T>.value;
    }

    public static implicit operator TypeID(Type type)
    {
        return new TypeID(type);
    }

    public static class Cache<T>
    {
        public readonly static TypeID value = new(typeof(T));
    }
}

/// <summary>
/// Represents a <see cref="System.Type"/> object.
/// </summary>
[Serializable]
public struct TypeID<T>
{
    public ulong value;

    public Type Type
    {
        get
        {
            if (TypeTable.TryGet(value, out Type type))
            {
                return type;
            }

            return null;
        }
    }

    public readonly override string ToString()
    {
        if (TypeTable.TryGet(value, out Type type))
        {
            return type.ToString();
        }

        return value.ToString();
    }

    public static implicit operator TypeID(TypeID<T> typeId)
    {
        TypeID newTypeId = default;
        newTypeId.value = typeId.value;
        return newTypeId;
    }
}