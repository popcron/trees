namespace Scripting
{
    public class ReadingFieldOfNullInstance : InterpreterException
    {
        public readonly MemberAccess memberAccess;
        public readonly Value target;

        public override string Message
        {
            get
            {
                return $"Unable to access '{memberAccess.member}', because the object that contains it is null";
            }
        }

        public ReadingFieldOfNullInstance(MemberAccess memberAccess, Value target, Interpreter.State state) : base(state)
        {
            this.memberAccess = memberAccess;
            this.target = target;
        }
    }
}