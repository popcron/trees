namespace Scripting
{
    public class AccessMemberNotSupported : InterpreterException
    {
        public readonly Expression expression;
        public readonly Value target;

        public override string Message
        {
            get
            {
                if (expression is MemberAccess memberAccess)
                {
                    return $"Cannot access '{memberAccess.member}' of target '{target.type}', because it doesn't support them";
                }
                else if (expression is MemberAssignment memberAssignment)
                {
                    return $"Cannot assign '{memberAssignment.member}' of target '{target.type}', because it doesn't support them";
                }
                else
                {
                    return $"Cannot access member of type '{target.type}', because it doesn't support them";
                }
            }
        }

        public AccessMemberNotSupported(Expression expression, Value target, Interpreter.State state) : base(state)
        {
            this.expression = expression;
            this.target = target;
        }
    }
}