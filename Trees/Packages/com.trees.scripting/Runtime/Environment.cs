using System;
using System.Collections.Generic;

namespace Scripting
{
    public class Environment
    {
        public readonly ulong id;
        public readonly string label;

        private readonly List<string> names = new();
        private readonly List<Keyword> keys = new();
        private readonly Dictionary<ulong, Value> values = new();
        private readonly Dictionary<ulong, Func<Value>> bindings = new();

        public int Count => names.Count;
        public IReadOnlyList<Keyword> Keys => keys;
        public IReadOnlyList<string> Names => names;
        public IEnumerable<(string name, Value value)> Variables
        {
            get
            {
                for (int i = 0; i < Count; i++)
                {
                    yield return (names[i], values[keys[i].hash]);
                }
            }
        }

        public override string ToString()
        {
            return $"Environment({label})";
        }

        public void ThrowIfNotRegistered(Keyword name)
        {
            if (!Contains(name))
            {
                throw new NullReferenceException($"A variable with the name '{name}' is not registered");
            }
        }

        public override bool Equals(object obj)
        {
            return obj is Environment other && Equals(other);
        }

        public virtual bool Equals(Environment other)
        {
            if (this == other)
            {
                return true;
            }

            if (Count != values.Count)
            {
                return false;
            }

            for (int i = 0; i < Count; i++)
            {
                Keyword key = keys[i];
                Value left = values[key.hash];
                if (other.values.TryGetValue(key.hash, out Value right))
                {
                    if (!left.Equals(right))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            HashCode hashCode = new();
            for (int i = 0; i < Count; i++)
            {
                hashCode.Add(keys[i].hash);
            }

            return hashCode.ToHashCode();
        }

        public bool Contains(Keyword keyword)
        {
            return values.ContainsKey(keyword.hash);
        }

        public Value Get(Keyword keyword, Value defaultValue = default)
        {
            if (values.TryGetValue(keyword.hash, out Value value))
            {
                return value;
            }

            return defaultValue;
        }

        public bool TryGet(ReadOnlySpan<char> name, out Value value)
        {
            // todo: a different kind of error than above, probably deserves more detailed handling
            return values.TryGetValue(ScriptingLibrary.GetHash(name), out value);
        }
    }
}