using System;

public static class TextExtensions
{
    private const ulong FnvOffsetBasis = 14695981039346656037;
    private const ulong FnvPrime = 1099511628211;

    public static ulong GetLongHashCode(this ReadOnlySpan<char> text)
    {
        ulong result = FnvOffsetBasis;
        for (int i = 0; i < text.Length; i++)
        {
            result ^= text[i];
            result *= FnvPrime;
        }

        return result;
    }
}
