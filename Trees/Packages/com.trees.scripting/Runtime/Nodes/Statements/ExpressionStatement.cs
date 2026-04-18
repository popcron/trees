using System;
using System.Text;

namespace Scripting
{
    public class ExpressionStatement : Statement
    {
        public readonly Expression expression;

        public override int ChildCount => 1;

        public ExpressionStatement(Expression expression, Range range, Module module) : base(range, module)
        {
            this.expression = expression;
        }

        public override Node GetChild(int index)
        {
            return expression;
        }

        public override void Append(StringBuilder stringBuilder, int depth)
        {
            expression.Append(stringBuilder, depth);
        }
    }
}
