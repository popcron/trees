namespace Scripting
{
    public class ExpectedExpression : ParserException
    {
        public readonly Token actualToken;
        public readonly string message;

        public override string Message => message;

        public ExpectedExpression(Token actualToken, Parser.State state) : base(state)
        {
            this.actualToken = actualToken;
            message = $"Expected an expression but found '{actualToken.GetSourceCode(state.sourceCode).ToString()}' instead";
        }
    }
}