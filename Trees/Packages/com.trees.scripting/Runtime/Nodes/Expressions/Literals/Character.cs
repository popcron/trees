using System;
using System.Text;

namespace Scripting
{
    public class Character : Literal
    {
        public readonly char value;

        public Character(char value, Range range, Module module) : base(range, module)
        {
            this.value = value;
        }

        public override void Append(StringBuilder stringBuilder, int depth)
        {
            stringBuilder.Append('\'');
            if (value == '\'')
            {
                stringBuilder.Append("\\'");
            }
            else if (value == '\\')
            {
                stringBuilder.Append("\\\\");
            }
            else
            {
                stringBuilder.Append(value);
            }

            stringBuilder.Append('\'');
        }
    }
}
