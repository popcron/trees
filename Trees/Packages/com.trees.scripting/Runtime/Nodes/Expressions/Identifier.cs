using System;
using System.Text;

namespace Scripting
{
    public class Identifier : Expression
    {
        public readonly string value;

        public Identifier(string value, Range range, Module file) : base(range, file)
        {
            this.value = value;
        }

        public override void Append(StringBuilder stringBuilder, int depth)
        {
            stringBuilder.Append(value);
        }
    }
}