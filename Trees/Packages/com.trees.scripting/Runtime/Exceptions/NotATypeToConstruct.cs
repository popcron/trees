namespace Scripting
{
    public class NotATypeToConstruct : InterpreterException
    {
        public readonly string name;
        public readonly Value.Type actualType;

        public override string Message => $"'{name}' is not a type (it is {actualType}) and cannot be constructed";

        public NotATypeToConstruct(string name, Value.Type actualType, Interpreter.State state) : base(state)
        {
            this.name = name;
            this.actualType = actualType;
        }
    }
}
