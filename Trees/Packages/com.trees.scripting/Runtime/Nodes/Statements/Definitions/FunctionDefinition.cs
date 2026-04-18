using System;
using System.Text;

namespace Scripting
{
    public class FunctionDefinition : Definition
    {
        public readonly string name;
        public readonly Block body;

        public override int ChildCount => 1;

        public FunctionDefinition(string name, Block body, Range range, Module file) : base(range, file)
        {
            this.name = name;
            this.body = body;
        }

        public override Node GetChild(int index)
        {
            return body;
        }

        public override void Append(StringBuilder stringBuilder, int depth)
        {
            stringBuilder.Append(name);
        }
    }
}