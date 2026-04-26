using System;
using System.Text;

namespace Scripting
{
    public class FunctionDefinition : Definition
    {
        public readonly string name;
        public readonly int nameIndex;
        public readonly string[] parameters;
        public readonly int[] parameterIndices;
        public readonly Block body;
        public FunctionSymbol symbol;
        public int slotIndex;

        public override int ChildCount => 1;

        public FunctionDefinition(string name, string[] parameters, Block body, Range range, Module file) : base(range, file)
        {
            this.name = name;
            nameIndex = file.readOnly.Add(name);
            this.parameters = parameters;
            parameterIndices = new int[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                parameterIndices[i] = file.readOnly.Add(parameters[i]);
            }
            this.body = body;
        }

        public override Node GetChild(int index)
        {
            return body;
        }

        public override void Append(StringBuilder stringBuilder, int depth)
        {
            stringBuilder.Append(KeywordMap.FunctionDeclaration);
            stringBuilder.Append(' ');
            stringBuilder.Append(name);
            stringBuilder.Append('(');
            for (int i = 0; i < parameters.Length; i++)
            {
                if (i > 0)
                {
                    stringBuilder.Append(',');
                    stringBuilder.Append(' ');
                }

                stringBuilder.Append(KeywordMap.VariableDeclaration);
                stringBuilder.Append(' ');
                stringBuilder.Append(parameters[i]);
            }

            stringBuilder.Append(')');
            stringBuilder.Append(' ');
            body.Append(stringBuilder, depth);
        }
    }
}
