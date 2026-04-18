namespace Scripting
{
    public class ExpectedNameOfTypeToConstruct : ParserException
    {
        public readonly Token actualToken;

        public override string Message
        {
            get
            {
                return $"Expected name of type to construct, but got '{actualToken}' instead";
            }
        }

        public ExpectedNameOfTypeToConstruct(Token actualToken, Parser.State state) : base(state)
        {
            this.actualToken = actualToken;
        }
    }
}