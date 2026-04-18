using System;

public static class HashFunctions
{
    public static ulong GetFNV1AHash(this ReadOnlySpan<char> text)
    {
        const ulong FnvOffsetBasis = 14695981039346656037;
        const ulong FnvPrime = 1099511628211;
        ulong result = FnvOffsetBasis;
        for (int i = 0; i < text.Length; i++)
        {
            result ^= text[i];
            result *= FnvPrime;
        }

        return result;
    }

    public static ulong GetDJB2Hash(this ReadOnlySpan<char> text)
    {
        // good for alphanumeric strings
        ulong hash = 5381;
        for (int i = 0; i < text.Length; i++)
        {
            hash = (hash << 5) + hash + text[i];        // hash * 33 + c
        }

        return hash;
    }

    public static ulong GetRollingHash(this ReadOnlySpan<char> text, ulong baseValue = 31)
    {
        ulong hash = 0;
        ulong power = 1;
        for (int i = 0; i < text.Length; i++)
        {
            hash += text[i] * power;
            power *= baseValue;
        }

        return hash;
    }
}