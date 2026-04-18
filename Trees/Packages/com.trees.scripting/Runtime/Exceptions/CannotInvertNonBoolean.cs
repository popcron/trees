namespace Scripting
{
    public class CannotInvertNonBoolean : InterpreterException
    {
        public readonly Value operand;
        public readonly string name;

        public override string Message
        {
            get
            {
                if (name != null)
                {
                    return $"Unable to treat value of '{name}' ({operand.type}) as a boolean for inversion";
                }
                else
                {
                    return $"Unable to treat '{operand}' as a boolean for inversion";
                }
            }
        }

        public CannotInvertNonBoolean(Value operand, string name, Interpreter.State state) : base(state)
        {
            this.operand = operand;
            this.name = name;
        }
    }
}