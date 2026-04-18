using System;
using System.Text;

namespace Scripting
{
    public class Boolean : Literal
    {
        public readonly bool value;

        public Boolean(bool value, Range range, Module file) : base(range, file)
        {
            this.value = value;
        }

        public override void Append(StringBuilder stringBuilder, int depth)
        {
            stringBuilder.Append(value ? KeywordMap.True : KeywordMap.False);
        }
    }
}