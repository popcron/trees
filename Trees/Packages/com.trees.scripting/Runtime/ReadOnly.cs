using System;
using System.Collections.Generic;

namespace Scripting
{
    public class ReadOnly
    {
        public readonly List<string> strings = new();

        public int Add(string value)
        {
            for (int i = 0; i < strings.Count; i++)
            {
                if (strings[i] == value)
                {
                    return i;
                }
            }

            strings.Add(value);
            return strings.Count - 1;
        }

        public int Add(ReadOnlySpan<char> value)
        {
            for (int i = 0; i < strings.Count; i++)
            {
                if (strings[i].AsSpan().SequenceEqual(value))
                {
                    return i;
                }
            }

            string copy = value.ToString();
            strings.Add(copy);
            return strings.Count - 1;
        }

        public bool TryGetIndex(ReadOnlySpan<char> value, out int index)
        {
            for (int i = 0; i < strings.Count; i++)
            {
                if (strings[i].AsSpan().SequenceEqual(value))
                {
                    index = i;
                    return true;
                }
            }

            index = -1;
            return false;
        }
    }
}
