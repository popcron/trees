using System;
using System.Collections.Generic;

namespace Scripting
{
    public class TypeSymbol
    {
        public readonly string name;
        public readonly List<FieldSymbol> fields;
        public readonly List<FunctionSymbol> methods = new();
        public readonly List<TypeSymbol> nestedTypes = new();

        public int FieldCount => fields.Count;

        public TypeSymbol(ReadOnlySpan<char> name)
        {
            this.name = name.ToString();
            this.fields = new();
        }

        public TypeSymbol(ReadOnlySpan<char> name, List<FieldSymbol> fields)
        {
            this.name = name.ToString();
            this.fields = fields;
        }

        public TypeSymbol(ReadOnlySpan<char> name, FieldSymbol[] fields)
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

        public bool TryGetMethod(ReadOnlySpan<char> name, out FunctionSymbol method)
        {
            for (int i = 0; i < methods.Count; i++)
            {
                if (methods[i].name.AsSpan().SequenceEqual(name))
                {
                    method = methods[i];
                    return true;
                }
            }

            method = null;
            return false;
        }

        public bool TryGetNestedType(ReadOnlySpan<char> name, out TypeSymbol nested)
        {
            for (int i = 0; i < nestedTypes.Count; i++)
            {
                if (nestedTypes[i].name.AsSpan().SequenceEqual(name))
                {
                    nested = nestedTypes[i];
                    return true;
                }
            }

            nested = null;
            return false;
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
