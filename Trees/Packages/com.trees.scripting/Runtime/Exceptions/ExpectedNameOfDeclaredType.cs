namespace Scripting
{
    public class ExpectedNameOfDeclaredType : ParserException
    {
        public readonly Token actualToken;

        public override string Message
        {
            get
            {
                return $"Expected name of declared type, but found '{actualToken}' instead";
            }
        }

        public ExpectedNameOfDeclaredType(Token actualToken, Parser.State state) : base(state)
        {
            this.actualToken = actualToken;
        }
    }
}