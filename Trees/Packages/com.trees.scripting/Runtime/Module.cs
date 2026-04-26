using System;
using System.Collections.Generic;

namespace Scripting
{
    public class Module
    {
        public readonly ReadOnly readOnly;
        public readonly List<Statement> statements = new();
        public readonly Dictionary<int, int> localSlots = new();
        public readonly HashSet<int> functionNameIndices = new();
        public int localCount;

        public Module(ReadOnly readOnly)
        {
            this.readOnly = readOnly;
        }

        public IEnumerable<Node> GetDescendants()
        {
            Stack<Node> stack = new();
            foreach (Statement statement in statements)
            {
                stack.Push(statement);
            }

            while (stack.TryPop(out Node node))
            {
                yield return node;

                int childCount = node.ChildCount;
                for (int i = 0; i < childCount; i++)
                {
                    stack.Push(node.GetChild(i));
                }
            }
        }

        public Node GetParentOf(Expression expression)
        {
            foreach (Node node in GetDescendants())
            {
                if (node.ContainsChild(expression))
                {
                    return node;
                }
            }

            throw new InvalidOperationException($"Expression {expression} does not have a parent.");
        }
    }
}