using System;
using System.Collections.Generic;

public static class TypeTable
{
    public static readonly Dictionary<ulong, Type> map = new();
    public static TryGetDelegate fallback;

    public static ulong AddOrSet(Type type)
    {
        ulong key = type.GetLongHashCode();
        map[key] = type;
        return key;
    }

    public static bool TryGet(ulong key, out Type type)
    {
        if (map.TryGetValue(key, out type))
        {
            return true;
        }
        else
        {
            if (fallback != null && fallback(key, out type))
            {
                AddOrSet(type);
                return true;
            }

            type = null;
            return false;
        }
    }

    public delegate bool TryGetDelegate(ulong key, out Type type);
}