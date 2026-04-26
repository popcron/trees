using System;
using System.Collections.Generic;

public static class TypeExtensions
{
    public static readonly Dictionary<ulong, Type> types = new();

    public static ulong GetID(this Type type)
    {
        ReadOnlySpan<char> fullName = type.FullName.AsSpan();
        ulong hash = 5381;
        for (int i = 0; i < fullName.Length; i++)
        {
            hash = (hash << 5) + hash + fullName[i];
        }

        types[hash] = type;
        return hash;
    }
}
