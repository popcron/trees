using System;
using System.Collections.Generic;

namespace Scripting
{
    public static class Lexer
    {
        public const char HexadecimalIndicator = 'X';
        public const char BinaryIndicator = 'B';

        public static bool TryRead(ReadOnlySpan<char> text, out int start, out int length, out TokenType type, bool isStartOfText = true)
        {
            int position = 0;

            // skip whitespace, but stop at newlines
            while (position < text.Length)
            {
                char c = text[position];
                if (IsNewLine(c))
                {
                    break;
                }

                if (!IsWhiteSpace(c))
                {
                    break;
                }

                position++;
            }

            if (position >= text.Length)
            {
                start = default;
                length = default;
                type = default;
                return false;
            }

            start = position;
            char character = text[position];

            // emit newline token (consume all consecutive newlines/whitespace between lines)
            if (IsNewLine(character))
            {
                position++;
                while (position < text.Length && (IsNewLine(text[position]) || IsWhiteSpace(text[position])))
                {
                    position++;
                }

                length = position - start;
                type = TokenType.NewLine;
                return true;
            }
            if (character == '"')
            {
                // start reading text inside quotes
                position++;
                while (position < text.Length)
                {
                    char c = text[position];
                    if (c == '"')
                    {
                        position++; // include closing quote
                        break;
                    }
                    else if (c == '\\' && position + 1 < text.Length)
                    {
                        position += 2; // skip escaped character
                    }
                    else
                    {
                        position++;
                    }
                }

                length = position - start;
                type = TokenType.Text;
            }
            else if (character == '\'')
            {
                // start reading a single character
                position++;
                if (position < text.Length && text[position] == '\\')
                {
                    position += 2; // skip escaped character
                }
                else if (position < text.Length)
                {
                    position++;
                }

                if (position >= text.Length || text[position] != '\'')
                {
                    position--;
                }

                position++;
                length = position - start;
                type = TokenType.Character;
            }
            else if (IsAlpha(character))
            {
                // start reading keyword until non-alphanumeric character
                position++;
                while (position < text.Length)
                {
                    char c = text[position];
                    if (!IsAlphaNumeric(c) && c != '_')
                    {
                        break;
                    }

                    position++;
                }

                length = position - start;
                type = TokenType.Keyword;
            }
            else if (IsNumeric(character))
            {
                // start reading
                position++;
                if (position + 1 < text.Length && text[start] == '0')
                {
                    char next = text[start + 1];
                    if (next == HexadecimalIndicator || next == char.ToLower(HexadecimalIndicator))
                    {
                        position++;
                        while (position < text.Length && IsHexadecimal(text[position]))
                        {
                            position++;
                        }

                        // consume type suffix
                        while (position < text.Length && IsAlpha(text[position]))
                        {
                            position++;
                        }

                        length = position - start;
                        type = TokenType.Number;
                        return true;
                    }
                    else if (next == BinaryIndicator || next == char.ToLower(BinaryIndicator))
                    {
                        position++;
                        while (position < text.Length && IsBinary(text[position]))
                        {
                            position++;
                        }

                        // consume type suffix
                        while (position < text.Length && IsAlpha(text[position]))
                        {
                            position++;
                        }

                        length = position - start;
                        type = TokenType.Number;
                        return true;
                    }
                }

                bool hasDecimalPoint = false;
                while (position < text.Length)
                {
                    char c = text[position];
                    if (c == '.')
                    {
                        if (hasDecimalPoint)
                        {
                            break;
                        }

                        hasDecimalPoint = true;
                    }
                    else if (!IsNumeric(c))
                    {
                        break;
                    }

                    position++;
                }

                // consume type suffix (e.g. f, u, ul, etc.)
                bool hasTypeSuffix = position < text.Length && IsAlpha(text[position]);
                while (position < text.Length && IsAlpha(text[position]))
                {
                    position++;
                }

                length = position - start;
                type = hasTypeSuffix ? TokenType.Keyword : TokenType.Number;
            }
            else if (character == '/')
            {
                // start reading comment
                if (position + 1 < text.Length && text[position + 1] == '/')
                {
                    position += 2; // skip "//"
                    while (position < text.Length)
                    {
                        char c = text[position];
                        if (c == '\n' || c == '\r')
                        {
                            break; // end of line, stop reading
                        }

                        position++;
                    }

                    length = position - start;
                    type = TokenType.Comment;
                }
                else
                {
                    length = start + 1 - start;
                    type = (TokenType)character;
                }
            }
            else
            {
                length = start + 1 - start;
                type = (TokenType)character;
            }

            return true;
        }

        public static void ReadAll(ReadOnlySpan<char> text, IList<Token> tokens)
        {
            int position = 0;
            while (position < text.Length)
            {
                if (TryRead(text[position..], out int start, out int length, out TokenType type, isStartOfText: position == 0))
                {
                    if (type != TokenType.Comment)
                    {
                        Token token = new(start + position, length, type);
                        tokens.Add(token);
                    }

                    position += start + length;
                }
                else
                {
                    break;
                }
            }
        }

        private static bool IsWhiteSpace(char character)
        {
            return character == (char)65279 || IsNewLine(character) || char.IsWhiteSpace(character);
        }

        private static bool IsNewLine(char character)
        {
            return character == '\n' || character == '\r';
        }

        private static bool IsAlpha(char character)
        {
            return (character >= 'a' && character <= 'z') || (character >= 'A' && character <= 'Z');
        }

        private static bool IsNumeric(char character)
        {
            return character >= '0' && character <= '9';
        }

        private static bool IsAlphaNumeric(char character)
        {
            return IsAlpha(character) || IsNumeric(character);
        }

        private static bool IsHexadecimal(char character)
        {
            return (character >= '0' && character <= '9') || (character >= 'a' && character <= 'f') || (character >= 'A' && character <= 'F');
        }

        private static bool IsBinary(char character)
        {
            return character == '0' || character == '1';
        }
    }

}