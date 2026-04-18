namespace Scripting
{
    public class DuplicateType : InterpreterException
    {
        public readonly TypeDefinition definition;

        public override string Message
        {
            get
            {
                return $"Type '{definition.name}' is already defined.";
            }
        }

        public DuplicateType(TypeDefinition definition, Interpreter.State state) : base(state)
        {
            this.definition = definition;
        }
    }
}