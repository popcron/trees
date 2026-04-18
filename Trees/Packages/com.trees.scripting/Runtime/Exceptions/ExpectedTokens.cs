namespace Scripting
{
    public class ExpectedTokens : ParserException
    {
        public override string Message
        {
            get
            {
                return "Expected more tokens but reached end of file";
            }
        }

        public ExpectedTokens(Parser.State state) : base(state)
        {
        }
    }
}