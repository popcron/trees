namespace Scripting
{
    public class OperatorNotSupportedBetweenBooleansAndCharacters : InterpreterException
    {
        public readonly Binary binary;

        public override string Message
        {
            get
            {
                return $"Operator '{binary.op}' is not supported between boolean and character values";
            }
        }

        public OperatorNotSupportedBetweenBooleansAndCharacters(Binary binary, Interpreter.State state) : base(state)
        {
            this.binary = binary;
        }
    }
}