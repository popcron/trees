using System;
using System.Collections.Generic;

namespace Scripting
{
    public class TypeSymbol
    {
        public readonly string name;
        public readonly List<FieldSymbol> fields;

        public int FieldCount => fields.Count;

        public TypeSymbol(ReadOnlySpan<char> name, List<FieldSymbol> fields)
        {
            this.name = name.ToString();
            this.fields = fields;
        }

        public TypeSymbol(ReadOnlySpan<char> name, IEnumerable<FieldSymbol> fields)
        {
            this.name = name.ToString();
            this.fields = new(fields);
        }

        public void ThrowIfFieldIsMissing(ReadOnlySpan<char> name)
        {
            for (int i = 0; i < FieldCount; i++)
            {
                if (fields[i].name.AsSpan().SequenceEqual(name))
                {
                    return;
                }
            }

            throw new Exception($"Type {this.name} does not contain field {name.ToString()}");
        }

        public int IndexOfField(ReadOnlySpan<char> name)
        {
            for (int i = 0; i < FieldCount; i++)
            {
                if (fields[i].name.AsSpan().SequenceEqual(name))
                {
                    return i;
                }
            }

            return -1;
        }

        public bool ContainsField(ReadOnlySpan<char> name)
        {
            for (int i = 0; i < FieldCount; i++)
            {
                if (fields[i].name.AsSpan().SequenceEqual(name))
                {
                    return true;
                }
            }

            return false;
        }

        public TypeSymbol Clone(ReadOnlySpan<char> newName)
        {
            List<FieldSymbol> newFields = new();
            for (int i = 0; i < FieldCount; i++)
            {
                newFields.Add(fields[i].Clone());
            }

            return new TypeSymbol(newName, newFields);
        }

        public FieldSymbol GetField(ReadOnlySpan<char> field)
        {
            return fields[IndexOfField(field)];
        }

        public bool TryGetField(ReadOnlySpan<char> field, out FieldSymbol result)
        {
            int index = IndexOfField(field);
            if (index != -1)
            {
                result = fields[index];
                return true;
            }

            result = null;
            return false;
        }
    }
}
