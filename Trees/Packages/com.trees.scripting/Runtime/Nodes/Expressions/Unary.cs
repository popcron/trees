using System;
using System.Text;

namespace Scripting
{
    public class Unary : Expression
    {
        public readonly Expression operand;
        public readonly UnaryOperator op;

        public override int ChildCount => 1;

        public Unary(Expression operand, UnaryOperator op, Range range, Module file) : base(range, file)
        {
            if (op == default)
            {
                throw new ArgumentNullException($"Given operator {op} is not a valid unary operator.");
            }

            this.operand = operand;
            this.op = op;
        }

        public override Node GetChild(int index)
        {
            return operand;
        }

        public override void Append(StringBuilder stringBuilder, int depth)
        {
            if (op == UnaryOperator.Negate)
            {
                stringBuilder.Append('-');
            }
            else if (op == UnaryOperator.Not)
            {
                stringBuilder.Append('!');
            }

            operand.Append(stringBuilder, depth);
        }
    }
}