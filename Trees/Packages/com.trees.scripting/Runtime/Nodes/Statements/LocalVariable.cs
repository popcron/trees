using System;
using System.Text;

namespace Scripting
{
    public class LocalVariable : Statement
    {
        public readonly string name;
        public readonly Expression initializer;

        public override int ChildCount => initializer == null ? 0 : 1;

        public LocalVariable(string name, Expression initializer, Range range, Module file) : base(range, file)
        {
            this.name = name;
            this.initializer = initializer;
        }

        public override Node GetChild(int index)
        {
            return initializer;
        }

        public override void Append(StringBuilder stringBuilder, int depth)
        {
            stringBuilder.Append(KeywordMap.VariableDeclaration);
            stringBuilder.Append(' ');
            stringBuilder.Append(name);
            if (initializer != null)
            {
                stringBuilder.Append(' ');
                stringBuilder.Append('=');
                stringBuilder.Append(' ');
                initializer.Append(stringBuilder, depth);
            }
        }
    }
}