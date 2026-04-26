using System;
using System.Text;

namespace Scripting
{
    public class TypeDefinition : Definition
    {
        public readonly string name;
        public readonly int nameIndex;
        public readonly FieldDefinition[] fields;
        public readonly FunctionDefinition[] methods;
        public readonly TypeDefinition[] types;
        public TypeSymbol symbol;
        public int slotIndex;

        public TypeDefinition(string name, FieldDefinition[] fields, FunctionDefinition[] methods, TypeDefinition[] types, Range range, Module module) : base(range, module)
        {
            this.name = name;
            nameIndex = module.readOnly.Add(name);
            this.fields = fields;
            this.methods = methods;
            this.types = types;
        }

        public override void Append(StringBuilder stringBuilder, int depth)
        {
            stringBuilder.Append(KeywordMap.TypeDeclaration);
            stringBuilder.Append(' ');
            stringBuilder.Append(name);
            stringBuilder.Append(' ');
            stringBuilder.Append('{');
            depth++;
            if (fields.Length > 0 || methods.Length > 0 || types.Length > 0)
            {
                for (int f = 0; f < fields.Length; f++)
                {
                    stringBuilder.AppendLine();
                    fields[f].Append(stringBuilder, depth);
                }

                for (int m = 0; m < methods.Length; m++)
                {
                    stringBuilder.AppendLine();
                    methods[m].Append(stringBuilder, depth);
                }

                for (int t = 0; t < types.Length; t++)
                {
                    stringBuilder.AppendLine();
                    types[t].Append(stringBuilder, depth);
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
