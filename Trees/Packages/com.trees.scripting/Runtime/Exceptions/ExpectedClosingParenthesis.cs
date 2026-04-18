namespace Scripting
{
    public class ExpectedClosingParenthesis : ParserException
    {
        public ExpectedClosingParenthesis(Parser.State state) : base(state)
        {
        }
    }
}