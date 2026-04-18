namespace Scripting
{
    public class DuplicateLocalVariable : InterpreterException
    {
        public readonly string name;

        public override string Message
        {
            get
            {
                return $"Local variable '{name}' is already defined in this scope.";
            }
        }

        public DuplicateLocalVariable(string name, Interpreter.State state) : base(state)
        {
            this.name = name;
        }
    }
}