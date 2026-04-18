namespace Scripting
{
    public class WritingFieldOfNullInstance : InterpreterException
    {
        public readonly MemberAssignment memberAssign;
        public readonly Value target;

        public override string Message
        {
            get
            {
                return $"Unable to modify '{memberAssign.member}', because the object that contains it is null";
            }
        }

        public WritingFieldOfNullInstance(MemberAssignment memberAssign, Value target, Interpreter.State state) : base(state)
        {
            this.memberAssign = memberAssign;
            this.target = target;
        }
    }
}