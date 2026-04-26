namespace Scripting
{
    public interface ITypeHandler<T>
    {
        /// <summary>
        /// Serializes the given <paramref name="value"/> into a <see cref="Value"/> that can be deserialized back into the original value.
        /// </summary>
        Value Serialize(T value);

        /// <summary>
        /// Deserializes the given <paramref name="value"/> into a <typeparamref name="T"/>.
        /// </summary>
        T Deserialize(Value value);
    }
}