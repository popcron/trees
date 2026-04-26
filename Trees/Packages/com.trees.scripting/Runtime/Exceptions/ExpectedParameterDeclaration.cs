namespace Scripting
{
    public class ExpectedParameterDeclaration : ParserException
    {
        public readonly string actualToken;

        public override string Message => $"Expected parameter declaration keyword 'var', but got '{actualToken}' instead";

        public ExpectedParameterDeclaration(Token actualToken, Parser.State state) : base(state)
        {
            this.actualToken = actualToken.GetSourceCode(state.sourceCode).ToString();
        }
    }
}
