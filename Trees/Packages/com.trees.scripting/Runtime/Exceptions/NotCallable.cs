namespace Scripting
{
    public class NotCallable : InterpreterException
    {
        public readonly Value.Type actualType;

        public override string Message => $"Value of type '{actualType}' is not callable";

        public NotCallable(Value.Type actualType, Interpreter.State state) : base(state)
        {
            this.actualType = actualType;
        }
    }
}
