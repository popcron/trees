using System;
using System.Text;

namespace Scripting
{
    public static class SyntaxHighlighter
    {
        public static void Highlight(Interpreter interpreter, ReadOnlySpan<char> source, StringBuilder stringBuilder)
        {
            if (source.IsEmpty)
            {
                return;
            }

            int i = 0;
            while (i < source.Length)
            {
                if (i + 1 < source.Length && source[i] == '/' && source[i + 1] == '/')
                {
                    int start = i;
                    while (i < source.Length && source[i] != '\n')
                    {
                        i++;
                    }

                    stringBuilder.Append("<color=").Append(SyntaxColors.CommentColor).Append('>');
                    stringBuilder.Append(source[start..i]);
                    stringBuilder.Append("</color>");
                }
                else if (source[i] == '"')
                {
                    int start = i;
                    i++; // opening quote
                    while (i < source.Length && source[i] != '"')
                    {
                        if (source[i] == '\\' && i + 1 < source.Length)
                        {
                            i += 2;
                        }
                        else
                        {
                            i++;
                        }
                    }

                    if (i < source.Length && source[i] == '"')
                    {
                        i++; // closing quote
                    }

                    stringBuilder.Append("<color=").Append(SyntaxColors.StringColor).Append('>');
                    stringBuilder.Append(source[start..i]);
                    stringBuilder.Append("</color>");
                }
                else if (source[i] == '\'')
                {
                    int start = i;
                    i++; // opening quote
                    if (i < source.Length && source[i] == '\\' && i + 1 < source.Length)
                    {
                        i += 2;
                    }
                    else if (i < source.Length && source[i] != '\'')
                    {
                        i++;
                    }

                    if (i < source.Length && source[i] == '\'')
                    {
                        i++; // closing quote
                    }

                    stringBuilder.Append("<color=").Append(SyntaxColors.StringColor).Append('>');
                    stringBuilder.Append(source[start..i]);
                    stringBuilder.Append("</color>");
                }
                else if (char.IsLetter(source[i]) || source[i] == '_')
                {
                    int start = i;
                    while (i < source.Length && (char.IsLetterOrDigit(source[i]) || source[i] == '_'))
                    {
                        i++;
                    }

                    ReadOnlySpan<char> word = source[start..i];
                    if (KeywordMap.IsKeyword(word))
                    {
                        stringBuilder.Append("<color=").Append(SyntaxColors.KeywordColor).Append('>');
                        stringBuilder.Append(word);
                        stringBuilder.Append("</color>");
                    }
                    else if (interpreter.ContainsFunctionVariable(word) || interpreter.ContainsFunctionBinding(word))
                    {
                        stringBuilder.Append("<color=").Append(SyntaxColors.FunctionColor).Append('>');
                        stringBuilder.Append(word);
                        stringBuilder.Append("</color>");
                    }
                    else if (interpreter.ContainsVariable(word))
                    {
                        stringBuilder.Append("<color=").Append(SyntaxColors.VariableColor).Append('>');
                        stringBuilder.Append(word);
                        stringBuilder.Append("</color>");
                    }
                    else if (interpreter.ContainsBinding(word))
                    {
                        stringBuilder.Append("<color=").Append(SyntaxColors.BindingColor).Append('>');
                        stringBuilder.Append(word);
                        stringBuilder.Append("</color>");
                    }
                    else
                    {
                        stringBuilder.Append(word);
                    }
                }
                else if (char.IsDigit(source[i]))
                {
                    int start = i;
                    while (i < source.Length && (char.IsDigit(source[i]) || source[i] == '.'))
                    {
                        i++;
                    }

                    stringBuilder.Append("<color=").Append(SyntaxColors.NumberColor).Append('>');
                    stringBuilder.Append(source[start..i]);
                    stringBuilder.Append("</color>");
                }
                else
                {
                    // escape '<'
                    if (source[i] == '<')
                    {
                        stringBuilder.Append("\\<");
                    }
                    else
                    {
                        stringBuilder.Append(source[i]);
                    }

                    i++;
                }
            }
        }
    }
}
