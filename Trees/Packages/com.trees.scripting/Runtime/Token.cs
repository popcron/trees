using System;

namespace Scripting
{
    public readonly struct Token
    {
        public readonly Range range;
        public readonly TokenType type;

        public Token(Range range, TokenType type)
        {
            this.range = range;
            this.type = type;
        }

        public Token(int start, int length, TokenType type)
        {
            this.range = new Range(start, start + length);
            this.type = type;
        }

        public readonly override string ToString()
        {
            if (IsControlToken(type))
            {
                return $"<{type}>";
            }
            else
            {
                return ((char)type).ToString();
            }
        }

        public readonly ReadOnlySpan<char> GetSourceCode(ReadOnlySpan<char> sourceCode)
        {
            return sourceCode[range];
        }

        public static bool IsControlToken(TokenType type)
        {
            return (int)type <= (int)TokenType.Control;
        }
    }
}