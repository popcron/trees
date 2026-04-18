using System;
using System.Collections.Generic;

public static class TypeExtensions
{
    public static readonly Dictionary<ulong, Type> types = new();

    public static ulong GetID(this Type type)
    {
        ulong hash = type.FullName.AsSpan().GetDJB2Hash();
        types[hash] = type;
        return hash;
    }
}
