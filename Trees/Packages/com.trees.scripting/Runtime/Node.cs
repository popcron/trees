using System;
using System.Text;

namespace Scripting
{
    public abstract class Node
    {
        public readonly Range range;
        public readonly Module module;

        public virtual int ChildCount { get; }

        protected Node(Range range, Module module)
        {
            this.range = range;
            this.module = module;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = StringBuilderPool.Rent();
            Append(stringBuilder, 0);
            return StringBuilderPool.ToStringAndReturn(stringBuilder);
        }

        public ReadOnlySpan<char> GetSourceCode(ReadOnlySpan<char> sourceCode)
        {
            return sourceCode[range];
        }

        public virtual Node GetChild(int index)
        {
            throw new InvalidOperationException($"Node of type {GetType().Name} does not have children.");
        }

        public bool ContainsChild(Node other)
        {
            int childCount = ChildCount;
            for (int i = 0; i < childCount; i++)
            {
                if (GetChild(i) == other)
                {
                    return true;
                }
            }

            return false;
        }

        public abstract void Append(StringBuilder stringBuilder, int depth);
    }
}