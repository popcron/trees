namespace Scripting
{
    public interface ITypeHandler<T>
    {
        /// <summary>
        /// Serializes the given <paramref name="value"/> into a <see cref="Value"/> that can be deserialized back into the original value.
        /// </summary>
        Value Serialize(T value);

        /// <summary>
        /// Tries to deserialize the given <paramref name="value"/> into a real <paramref name="result"/>.
        /// </summary>
        bool TryDeserialize(Value value, out T result);
    }
}