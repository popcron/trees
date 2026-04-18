namespace Scripting
{
    public class CannotNegateNonNumber : InterpreterException
    {
        public readonly Value operand;
        public readonly string name;

        public override string Message
        {
            get
            {
                if (name != null)
                {
                    return $"Cannot negate non-number value of '{name}' ({operand})";
                }
                else
                {
                    return $"Cannot negate non-number value '{operand}'";
                }
            }
        }

        public CannotNegateNonNumber(Value operand, string name, Interpreter.State state) : base(state)
        {
            this.operand = operand;
            this.name = name;
        }
    }
}