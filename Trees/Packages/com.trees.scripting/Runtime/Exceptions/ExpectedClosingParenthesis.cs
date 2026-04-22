namespace Scripting
{
    public class ExpectedClosingParenthesis : ParserException
    {
        public readonly string actualToken;

        public override string Message
        {
            get
            {
                return $"Expected ) to close a parenthesized expression, but found '{actualToken}'";
            }
        }

        public ExpectedClosingParenthesis(Token actualToken, Parser.State state) : base(state)
        {
            this.actualToken = actualToken.GetSourceCode(state.sourceCode).ToString();
        }
    }
}