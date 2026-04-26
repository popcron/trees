namespace Scripting
{
    public class FunctionSymbol
    {
        public readonly string name;
        public readonly string[] parameters;
        public readonly int[] parameterIndices;
        public readonly Block body;
        public readonly bool isMethod;
        public readonly int frameSize;

        public int ParameterCount => parameters.Length;

        public FunctionSymbol(FunctionDefinition definition, bool isMethod, int frameSize)
        {
            name = definition.name;
            parameters = definition.parameters;
            parameterIndices = definition.parameterIndices;
            body = definition.body;
            this.isMethod = isMethod;
            this.frameSize = frameSize;
        }
    }
}
