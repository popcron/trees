using System;
using System.Collections.Generic;
using System.Text;

namespace Scripting
{
    public class Construction : Expression
    {
        public readonly string type;
        public readonly List<(string field, Expression value)> arguments;

        public override int ChildCount => arguments.Count;

        public Construction(string type, List<(string field, Expression value)> arguments, Range range, Module module) : base(range, module)
        {
            this.type = type;
            this.arguments = arguments;
        }

        public override Node GetChild(int index)
        {
            return arguments[index].value;
        }

        public override void Append(StringBuilder stringBuilder, int depth)
        {
            stringBuilder.Append(KeywordMap.CreateInstance);
            stringBuilder.Append(' ');
            stringBuilder.Append(type);
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
