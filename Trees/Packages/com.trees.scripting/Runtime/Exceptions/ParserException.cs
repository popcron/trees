using System;
using System.Collections.Generic;

namespace Scripting
{
    public abstract class ParserException : Exception
    {
        public readonly int tokenIndex;
        public readonly List<Token> tokens;

        public ParserException(Parser.State state)
        {
            tokenIndex = state.tokenIndex;
            tokens = state.tokens;
        }
    }
}