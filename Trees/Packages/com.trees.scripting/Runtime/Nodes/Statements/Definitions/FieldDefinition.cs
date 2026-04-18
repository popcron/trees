using System;
using System.Text;

namespace Scripting
{
    public class FieldDefinition : Definition
    {
        public readonly Value.Type type;
        public readonly string name;

        public FieldDefinition(Value.Type type, string name, Range range, Module module) : base(range, module)
        {
            this.type = type;
            this.name = name;
        }

        public override void Append(StringBuilder stringBuilder, int depth)
        {
            stringBuilder.Append(KeywordMap.FieldDeclaration);
            stringBuilder.Append(' ');
            stringBuilder.Append(name);
            if (type != default)
            {
                stringBuilder.Append(':');
                stringBuilder.Append(' ');
                stringBuilder.Append(type);
            }
        }
    }
}
