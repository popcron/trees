public static class LongHashCode<T>
{
    public static readonly ulong value = typeof(T).GetID();
}