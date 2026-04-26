using System;
using System.Collections.Generic;
using System.Text;

namespace Scripting
{
    public class ObjectInstance
    {
        public readonly TypeSymbol typeSymbol;
        public readonly Scope declaringScope;
        public readonly Value[] fields;
        public readonly Dictionary<ulong, Action<Value>> callbacks = new();

        public ObjectInstance(TypeSymbol typeSymbol, Scope declaringScope)
        {
            this.typeSymbol = typeSymbol;
            this.declaringScope = declaringScope;
            fields = new Value[typeSymbol.fields.Count];
            for (int i = 0; i < fields.Length; i++)
            {
                fields[i] = new(typeSymbol.fields[i].type);
            }
        }

        public ObjectInstance(TypeSymbol typeSymbol, Value[] fields, Scope declaringScope)
        {
            this.typeSymbol = typeSymbol;
            this.fields = fields;
            this.declaringScope = declaringScope;
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
            stringBuilder.Append(typeSymbol.name);
            stringBuilder.Append('(');
            if (typeSymbol.fields.Count > 0)
            {
                stringBuilder.Append('\n');
                for (int i = 0; i < typeSymbol.fields.Count; i++)
                {
                    FieldSymbol field = typeSymbol.fields[i];
                    stringBuilder.Append(new string(' ', depth * 4));
                    stringBuilder.Append(field.name);
                    stringBuilder.Append(' ');
                    stringBuilder.Append('=');
                    stringBuilder.Append(' ');
                    Value value = fields[i];
                    value.Append(stringBuilder, depth + 1);

                    if (i < typeSymbol.fields.Count - 1)
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
            typeSymbol.ThrowIfFieldIsMissing(name);

            fields[typeSymbol.IndexOfField(name)] = value;
        }

        public void Set<T>(ReadOnlySpan<char> name, T value)
        {
            Set(name, Value.Serialize(value));
        }

        public Value Get(ReadOnlySpan<char> name)
        {
            typeSymbol.ThrowIfFieldIsMissing(name);

            return fields[typeSymbol.IndexOfField(name)];
        }

        public T Get<T>(ReadOnlySpan<char> name)
        {
            return Get(name).Deserialize<T>();
        }

        // todo: these "readers" are slotted to the same indices as fields, so the dictionary isnt needed
        public void DeclareReader(ReadOnlySpan<char> name, Action<Value> callback)
        {
            ulong nameHash = 5381;
            for (int i = 0; i < name.Length; i++)
            {
                nameHash = (nameHash << 5) + nameHash + name[i];
            }

            if (callbacks.ContainsKey(nameHash))
            {
                throw new ArgumentException($"A callback with the name '{name.ToString()}' is already registered");
            }

            callbacks[nameHash] = callback;
        }

        public bool TryGetCallback(ReadOnlySpan<char> name, out Action<Value> callback)
        {
            ulong nameHash = 5381;
            for (int i = 0; i < name.Length; i++)
            {
                nameHash = (nameHash << 5) + nameHash + name[i];
            }

            return callbacks.TryGetValue(nameHash, out callback);
        }
    }
}
