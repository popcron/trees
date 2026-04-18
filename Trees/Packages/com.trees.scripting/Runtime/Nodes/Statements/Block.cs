using System;
using System.Collections.Generic;
using System.Text;

namespace Scripting
{
    public class Block : Statement
    {
        public readonly List<Statement> statements = new();

        public override int ChildCount => statements.Count;

        public Block(Range range, Module file) : base(range, file)
        {
        }

        public override Node GetChild(int index)
        {
            return statements[index];
        }

        public override void Append(StringBuilder stringBuilder, int depth)
        {
            stringBuilder.Append('{');
            if (statements.Count > 0)
            {
                stringBuilder.Append('\n');
                depth++;

                for (int i = 0; i < statements.Count; i++)
                {
                    stringBuilder.Append(new string(' ', depth * 4));
                    statements[i].Append(stringBuilder, depth);
                    stringBuilder.Append('\n');
                }

                stringBuilder.Append('\n');
                depth--;
                stringBuilder.Append(new string(' ', depth * 4));
            }
            else
            {
                stringBuilder.Append(' ');
            }

            stringBuilder.Append('}');
        }
    }
}