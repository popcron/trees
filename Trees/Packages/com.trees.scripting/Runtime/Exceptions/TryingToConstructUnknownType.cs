namespace Scripting
{
    public class TryingToConstructUnknownType : InterpreterException
    {
        public readonly string type;

        public TryingToConstructUnknownType(string type, Interpreter.State state) : base(state)
        {
            this.type = type;
        }
    }
}