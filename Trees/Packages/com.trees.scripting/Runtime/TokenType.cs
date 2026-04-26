namespace Scripting
{
    public enum TokenType
    {
        Uninitialized = 0,
        Text,
        Character,
        Keyword,
        Number,
        Comment,
        NewLine,

        /// <summary>
        /// Enum values after this are characters.
        /// </summary>
        Control,

        Semicolon = ';',
        Equals = '=',
        Minus = '-',
        Plus = '+',
        Asterisk = '*',
        Slash = '/',
        Percent = '%',
        Bang = '!',
        Dot = '.',
        Comma = ',',
        OpenParenthesis = '(',
        CloseParenthesis = ')',
        OpenBrace = '{',
        CloseBrace = '}',
    }
}