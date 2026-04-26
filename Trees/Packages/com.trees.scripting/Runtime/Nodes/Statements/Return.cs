using System;
using System.Text;

namespace Scripting
{
    public class Return : Statement
    {
        public Expression value;

        public override int ChildCount => value == null ? 0 : 1;

        public Return(Expression value, Range range, Module file) : base(range, file)
        {
            this.value = value;
        }

        public override Node GetChild(int index)
        {
            return value;
        }

        public override void Append(StringBuilder stringBuilder, int depth)
        {
            stringBuilder.Append(KeywordMap.Return);
            if (value != null)
            {
                stringBuilder.Append(' ');
                value.Append(stringBuilder, depth);
            }
        }
    }
}