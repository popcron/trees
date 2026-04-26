using System;
using System.Collections.Generic;
using System.Text;

namespace Scripting
{
    public class Construction : Expression
    {
        public Expression type;
        public readonly List<(string field, Expression value)> arguments;

        public override int ChildCount => 1 + arguments.Count;

        public Construction(Expression type, List<(string field, Expression value)> arguments, Range range, Module module) : base(range, module)
        {
            this.type = type;
            this.arguments = arguments;
        }

        public override Node GetChild(int index)
        {
            if (index == 0)
            {
                return type;
            }

            return arguments[index - 1].value;
        }

        public override void Append(StringBuilder stringBuilder, int depth)
        {
            stringBuilder.Append(KeywordMap.CreateInstance);
            stringBuilder.Append(' ');
            type.Append(stringBuilder, depth);
            stringBuilder.Append('(');

            for (int i = 0; i < arguments.Count; i++)
            {
                (string field, Expression argument) = arguments[i];
                if (i > 0)
                {
                    stringBuilder.Append(", ");
                }

                stringBuilder.Append(field);
                stringBuilder.Append(" = ");
                argument.Append(stringBuilder, depth);
            }

            stringBuilder.Append(')');
        }
    }
}
