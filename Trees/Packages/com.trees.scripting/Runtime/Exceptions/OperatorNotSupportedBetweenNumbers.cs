namespace Scripting
{
    public class OperatorNotSupportedBetweenNumbers : InterpreterException
    {
        public readonly Binary binary;

        public override string Message
        {
            get
            {
                return $"Operator '{binary.op}' is not supported between number values";
            }
        }

        public OperatorNotSupportedBetweenNumbers(Binary binary, Interpreter.State state) : base(state)
        {
            this.binary = binary;
        }
    }
}