using System;
using System.Text;

namespace Scripting
{
    public class MemberAssignment : Expression
    {
        public readonly Expression target;
        public readonly string member;
        public readonly Expression value;

        public override int ChildCount => 2;

        public MemberAssignment(Expression target, string member, Expression value, Range range, Module module) : base(range, module)
        {
            this.target = target;
            this.member = member;
            this.value = value;
        }

        public override Node GetChild(int index)
        {
            return index == 0 ? target : value;
        }

        public override void Append(StringBuilder stringBuilder, int depth)
        {
            target.Append(stringBuilder, depth);
            stringBuilder.Append('.');
            stringBuilder.Append(member);
            stringBuilder.Append(" = ");
            value.Append(stringBuilder, depth);
        }
    }
}
