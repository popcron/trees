namespace Scripting
{
    public class ExpectedNameOfVariable : ParserException
    {
        public ExpectedNameOfVariable(Parser.State state) : base(state)
        {
        }
    }
}