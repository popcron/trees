using System;
using System.Text;

namespace Scripting
{
    public class MemberAccess : Expression
    {
        public Expression target;
        public readonly string member;

        public override int ChildCount => 1;

        public MemberAccess(Expression target, string member, Range range, Module module) : base(range, module)
        {
            this.target = target;
            this.member = member;
        }

        public override Node GetChild(int index)
        {
            return target;
        }

        public override void Append(StringBuilder stringBuilder, int depth)
        {
            target.Append(stringBuilder, depth);
            stringBuilder.Append('.');
            stringBuilder.Append(member);
        }
    }
}
