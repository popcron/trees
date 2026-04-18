namespace Scripting
{
    public class TooManyArgumentsForConstructor : InterpreterException
    {
        public readonly TypeSymbol type;
        public readonly Construction construction;

        public TooManyArgumentsForConstructor(TypeSymbol type, Construction construction, Interpreter.State state) : base(state)
        {
            this.type = type;
            this.construction = construction;
        }
    }
}