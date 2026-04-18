using System;
using System.Text;

namespace Scripting
{
    public class TypeDefinition : Definition
    {
        public readonly string name;
        public readonly FieldDefinition[] fields;

        public TypeDefinition(string name, FieldDefinition[] fields, Range range, Module module) : base(range, module)
        {
            this.name = name;
            this.fields = fields;
        }

        public override void Append(StringBuilder stringBuilder, int depth)
        {
            stringBuilder.Append(KeywordMap.TypeDeclaration);
            stringBuilder.Append(' ');
            stringBuilder.Append('{');
            depth++;
            if (fields.Length > 0)
            {
                for (int f = 0; f < fields.Length; f++)
                {
                    stringBuilder.AppendLine();
                    fields[f].Append(stringBuilder, depth);
                }
            }
            else
            {
                stringBuilder.Append(' ');
            }

            depth--;
            stringBuilder.Append('}');
        }
    }
}
