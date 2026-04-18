using System;

namespace Scripting
{
    public class Group : Expression
    {
        public readonly Expression expression;

        public override int ChildCount => 1;

        public Group(Expression expression, Range range, Module file) : base(range, file)
        {
            this.expression = expression;
        }

        public override Node GetChild(int index)
        {
            return expression;
        }

        public override void Append(System.Text.StringBuilder stringBuilder, int depth)
        {
            stringBuilder.Append('(');
            expression.Append(stringBuilder, depth);
            stringBuilder.Append(')');
        }
    }
}