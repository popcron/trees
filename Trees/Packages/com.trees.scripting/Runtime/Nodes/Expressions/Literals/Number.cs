using System;
using System.Text;

namespace Scripting
{
    public class Number : Literal
    {
        public readonly string value;

        public Number(string value, Range range, Module file) : base(range, file)
        {
            this.value = value;
        }

        public override void Append(StringBuilder stringBuilder, int depth)
        {
            stringBuilder.Append(value);
        }
    }
}