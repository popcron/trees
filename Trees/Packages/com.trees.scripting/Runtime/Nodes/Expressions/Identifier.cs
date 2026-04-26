using System;
using System.Text;

namespace Scripting
{
    public class Identifier : Expression
    {
        public readonly string value;
        public readonly int valueIndex;

        public Identifier(string value, Range range, Module file) : base(range, file)
        {
            this.value = value;
            valueIndex = file.readOnly.Add(value);
        }

        public override void Append(StringBuilder stringBuilder, int depth)
        {
            stringBuilder.Append(value);
        }
    }
}