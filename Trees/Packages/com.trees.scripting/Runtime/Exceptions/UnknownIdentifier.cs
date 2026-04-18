namespace Scripting
{
    public class UnknownIdentifier : InterpreterException
    {
        public readonly string name;

        public override string Message
        {
            get
            {
                return $"Unknown identifier '{name}'";
            }
        }

        public UnknownIdentifier(string name, Interpreter.State state) : base(state)
        {
            this.name = name;
        }
    }
}