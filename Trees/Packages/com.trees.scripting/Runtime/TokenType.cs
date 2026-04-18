namespace Scripting
{
    public enum TokenType
    {
        Text = 1,
        Character = 2,
        Keyword = 3,
        Number = 4,
        Comment = 5,
        NewLine = 6,

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
        DoubleEquals = 128,
        NotEquals,
        And,
        Or,
        Dot = '.',
        Comma = ',',
        OpenParenthesis = '(',
        CloseParenthesis = ')',
        OpenBrace = '{',
        CloseBrace = '}',
    }
}