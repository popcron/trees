namespace Scripting
{
    public class ExpectedNameOfFunction : ParserException
    {
        public override string Message => "Expected name of function after 'fn' keyword";

        public ExpectedNameOfFunction(Parser.State state) : base(state)
        {
        }
    }
}
