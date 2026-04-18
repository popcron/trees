using System;
using System.Text;

namespace Scripting
{
    public class If : Statement
    {
        public readonly Expression condition;
        public readonly Statement body;
        public readonly Statement elseBody;

        public override int ChildCount => elseBody == null ? 2 : 3;

        public If(Expression condition, Statement body, Statement elseBody, Range range, Module module) : base(range, module)
        {
            this.condition = condition;
            this.body = body;
            this.elseBody = elseBody;
        }

        public override Node GetChild(int index)
        {
            return index switch
            {
                0 => condition,
                1 => body,
                2 => elseBody,
                _ => throw new ArgumentOutOfRangeException(nameof(index))
            };
        }

        public override void Append(StringBuilder stringBuilder, int depth)
        {
            stringBuilder.Append(KeywordMap.If);
            stringBuilder.Append(' ');
            stringBuilder.Append('(');
            condition.Append(stringBuilder, depth);
            stringBuilder.Append(')');
            stringBuilder.Append(' ');
            body.Append(stringBuilder, depth);
            if (elseBody != null)
            {
                stringBuilder.Append(' ');
                stringBuilder.Append(KeywordMap.Else);
                stringBuilder.Append(' ');
                elseBody.Append(stringBuilder, depth);
            }
        }
    }
}
