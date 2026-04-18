using System;

public static class TypeExtensions
{
    public static ulong GetLongHashCode(this Type type)
    {
        string fullName = type.FullName ?? type.Name;
        return fullName.AsSpan().GetLongHashCode();
    }
}
