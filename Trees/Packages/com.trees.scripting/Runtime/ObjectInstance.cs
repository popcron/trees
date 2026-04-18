using System;
using System.Collections.Generic;
using System.Text;

namespace Scripting
{
    public class ObjectInstance
    {
        public readonly TypeSymbol type;
        public readonly Value[] values;
        public readonly Dictionary<ulong, Action<Value>> callbacks = new();

        public ObjectInstance(TypeSymbol type)
        {
            this.type = type;
            values = new Value[type.FieldCount];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = new(type.fields[i].type);
            }
        }

        public ObjectInstance(TypeSymbol type, Value[] values)
        {
            this.type = type;
            this.values = values;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = StringBuilderPool.Rent();
            Append(stringBuilder);
            return StringBuilderPool.ToStringAndReturn(stringBuilder);
        }

        public void Append(StringBuilder stringBuilder, int depth = 0)
        {
            stringBuilder.Append(KeywordMap.CreateInstance);
            stringBuilder.Append(' ');
            stringBuilder.Append(type.name);
            stringBuilder.Append('(');
            if (type.FieldCount > 0)
            {
                stringBuilder.Append('\n');
                for (int i = 0; i < type.FieldCount; i++)
                {
                    FieldSymbol field = type.fields[i];
                    stringBuilder.Append(new string(' ', depth * 4));
                    stringBuilder.Append(field.name);
                    stringBuilder.Append(' ');
                    stringBuilder.Append('=');
                    stringBuilder.Append(' ');
                    Value value = values[i];
                    if (value.objectValue == null)
                    {
                        stringBuilder.Append(KeywordMap.Null);
                    }
                    else
                    {
                        if (value.TryDeserialize(out ObjectInstance nestedStructure))
                        {
                            nestedStructure.Append(stringBuilder, depth + 1);
                        }
                        else
                        {
                            stringBuilder.Append(value);
                        }
                    }

                    if (i > 0)
                    {
                        stringBuilder.Append(',');
                    }

                    stringBuilder.Append('\n');
                }
            }

            stringBuilder.Append(')');
        }

        public void Set(ReadOnlySpan<char> name, Value value)
        {
            type.ThrowIfFieldIsMissing(name);

            values[type.IndexOfField(name)] = value;
        }

        public void Set<T>(ReadOnlySpan<char> name, T value)
        {
            Set(name, Value.Serialize(value));
        }

        public Value Get(ReadOnlySpan<char> name)
        {
            type.ThrowIfFieldIsMissing(name);

            return values[type.IndexOfField(name)];
        }

        public T Get<T>(ReadOnlySpan<char> name)
        {
            return Get(name).Deserialize<T>();
        }

        public void AddCallback(ReadOnlySpan<char> name, Action<Value> callback)
        {
            ulong hash = ScriptingLibrary.GetHash(name);
            if (callbacks.ContainsKey(hash))
            {
                throw new ArgumentException($"A callback with the name '{name.ToString()}' is already registered");
            }

            callbacks[hash] = callback;
        }
    }
}
