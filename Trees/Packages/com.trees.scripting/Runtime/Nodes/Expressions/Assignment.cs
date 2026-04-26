using System;
using System.Text;

namespace Scripting
{
    public class Assignment : Expression
    {
        public readonly string name;
        public readonly int nameIndex;
        public Expression value;
        public AssignmentTarget target;
        public int bindingIndex;
        public int frameDepth;
        public int slotIndex;

        public override int ChildCount => 1;

        public Assignment(string name, Expression value, Range range, Module module) : base(range, module)
        {
            this.name = name;
            nameIndex = module.readOnly.Add(name);
            this.value = value;
            target = AssignmentTarget.Unresolved;
            bindingIndex = -1;
            frameDepth = -1;
            slotIndex = -1;
        }

        public override Node GetChild(int index)
        {
            return value;
        }

        public override void Append(StringBuilder stringBuilder, int depth)
        {
            stringBuilder.Append(name);
            stringBuilder.Append(' ');
            stringBuilder.Append('=');
            stringBuilder.Append(' ');
            value.Append(stringBuilder, depth);
        }
    }
}
