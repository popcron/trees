using System;
using System.Collections.Generic;
using System.Text;

namespace Scripting
{
    public class Call : Expression
    {
        public Expression callee;
        public readonly List<Expression> arguments;

        public override int ChildCount => 1 + arguments.Count;

        public Call(Expression callee, List<Expression> arguments, Range range, Module module) : base(range, module)
        {
            this.callee = callee;
            this.arguments = arguments;
        }

        public override Node GetChild(int index)
        {
            if (index == 0)
            {
                return callee;
            }

            return arguments[index - 1];
        }

        public override void Append(StringBuilder stringBuilder, int depth)
        {
            callee.Append(stringBuilder, depth);
            stringBuilder.Append('(');
            for (int i = 0; i < arguments.Count; i++)
            {
                if (i > 0)
                {
                    stringBuilder.Append(',');
                    stringBuilder.Append(' ');
                }

                arguments[i].Append(stringBuilder, depth);
            }

            stringBuilder.Append(')');
        }
    }
}
