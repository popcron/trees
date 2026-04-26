namespace Scripting
{
    public class ExpectedNameOfParameter : ParserException
    {
        public override string Message => "Expected name of parameter after 'var' keyword";

        public ExpectedNameOfParameter(Parser.State state) : base(state)
        {
        }
    }
}
