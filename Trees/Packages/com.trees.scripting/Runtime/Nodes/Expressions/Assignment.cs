using System;
using System.Text;

namespace Scripting
{
    public class Assignment : Expression
    {
        public readonly string name;
        public readonly Expression value;

        public override int ChildCount => 1;

        public Assignment(string name, Expression value, Range range, Module module) : base(range, module)
        {
            this.name = name;
            this.value = value;
        }

        public override Node GetChild(int index)
        {
            return value;
        }

        public override void Append(StringBuilder stringBuilder, int depth)
        {
            stringBuilder.Append(name);
            stringBuilder.Append(' ');
            stringBuilder.Append('=');
            stringBuilder.Append(' ');
            value.Append(stringBuilder, depth);
        }
    }
}