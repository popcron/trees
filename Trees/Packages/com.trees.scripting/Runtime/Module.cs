using System;
using System.Collections.Generic;

namespace Scripting
{
    public class Module
    {
        public readonly List<Statement> statements = new();

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
            Stack<Node> stack = new();
            foreach (Statement statement in statements)
            {
                stack.Push(statement);
            }

            while (stack.TryPop(out Node node))
            {
                if (node.ContainsChild(expression))
                {
                    return node;
                }

                int childCount = node.ChildCount;
                for (int i = 0; i < childCount; i++)
                {
                    stack.Push(node.GetChild(i));
                }
            }

            throw new InvalidOperationException($"Expression {expression} does not have a parent.");
        }
    }
}