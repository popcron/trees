using System;
using System.Text;

namespace Scripting
{
    public class Binary : Expression
    {
        public readonly Expression left;
        public readonly Expression right;
        public readonly BinaryOperator op;

        public override int ChildCount => 2;

        public Binary(Expression left, Expression right, BinaryOperator op, Range range, Module file) : base(range, file)
        {
            if (op == default)
            {
                throw new ArgumentNullException($"Given operator {op} is not a valid binary operator.");
            }

            this.left = left;
            this.right = right;
            this.op = op;
        }

        public override Node GetChild(int index)
        {
            return index == 0 ? left : right;
        }

        public override void Append(StringBuilder stringBuilder, int depth)
        {
            left.Append(stringBuilder, depth);
            stringBuilder.Append(' ');
            if (op == BinaryOperator.Add)
            {
                stringBuilder.Append("+");
            }
            else if (op == BinaryOperator.Subtract)
            {
                stringBuilder.Append("-");
            }
            else if (op == BinaryOperator.Multiply)
            {
                stringBuilder.Append("*");
            }
            else if (op == BinaryOperator.Divide)
            {
                stringBuilder.Append("/");
            }
            else if (op == BinaryOperator.Modulus)
            {
                stringBuilder.Append("%");
            }
            else if (op == BinaryOperator.Equal)
            {
                stringBuilder.Append("==");
            }
            else if (op == BinaryOperator.NotEqual)
            {
                stringBuilder.Append("!=");
            }
            else if (op == BinaryOperator.And)
            {
                stringBuilder.Append("&&");
            }
            else if (op == BinaryOperator.Or)
            {
                stringBuilder.Append("||");
            }

            stringBuilder.Append(' ');
            right.Append(stringBuilder, depth);
        }
    }
}