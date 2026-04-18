using System;
using System.Text;

namespace Scripting
{
    public class NullLiteral : Literal
    {
        public NullLiteral(Range range, Module module) : base(range, module)
        {
        }

        public override void Append(StringBuilder stringBuilder, int depth)
        {
            stringBuilder.Append(KeywordMap.Null);
        }
    }
}
