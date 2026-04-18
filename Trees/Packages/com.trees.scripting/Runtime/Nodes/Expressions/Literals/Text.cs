using System;
using System.Text;

namespace Scripting
{
    public class Text : Literal
    {
        public readonly string value;

        public Text(string value, Range range, Module module) : base(range, module)
        {
            this.value = value;
        }

        public override void Append(StringBuilder stringBuilder, int depth)
        {
            stringBuilder.Append('"');
            stringBuilder.Append(value);
            stringBuilder.Append('"');
        }
    }
}
