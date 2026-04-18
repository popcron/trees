using System.Collections.Generic;
using System.Text;

namespace Scripting
{
    internal static class StringBuilderPool
    {
        private static readonly Stack<StringBuilder> pool = new();

        public static StringBuilder Rent()
        {
            if (pool.TryPop(out StringBuilder cachedStringBuilder))
            {
                return cachedStringBuilder;
            }
            else
            {
                return new StringBuilder();
            }
        }

        public static void Return(StringBuilder cachedStringBuilder)
        {
            cachedStringBuilder.Clear();
            pool.Push(cachedStringBuilder);
        }

        public static string ToStringAndReturn(StringBuilder cachedStringBuilder)
        {
            string result = cachedStringBuilder.ToString();
            Return(cachedStringBuilder);
            return result;
        }
    }
}
