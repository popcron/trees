namespace Scripting
{
    public class ArgumentCountMismatch : InterpreterException
    {
        public readonly string functionName;
        public readonly int expected;
        public readonly int actual;

        public override string Message => $"Function '{functionName}' expects {expected} argument(s) but got {actual}";

        public ArgumentCountMismatch(string functionName, int expected, int actual, Interpreter.State state) : base(state)
        {
            this.functionName = functionName;
            this.expected = expected;
            this.actual = actual;
        }
    }
}
