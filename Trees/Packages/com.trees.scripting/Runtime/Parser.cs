using System;
using System.Collections.Generic;

namespace Scripting
{
    public static class Parser
    {
        public static Module Parse(ReadOnlySpan<char> sourceCode)
        {
            Module module = new();
            Context context = new(module, sourceCode, 0);
            while (context.TryPeek(out Token nextToken))
            {
                if (nextToken.type == TokenType.NewLine || nextToken.type == TokenType.Semicolon)
                {
                    context.Advance();
                    continue;
                }

                Statement statement = ParseStatement(ref context);
                context.module.statements.Add(statement);
                context.SkipTerminators();
            }

            return context.module;
        }

        private static bool IsTerminator(TokenType type)
        {
            return type == TokenType.NewLine || type == TokenType.Semicolon;
        }

        private static Expression ParseExpression(ref Context context, int minPrecedence = 0)
        {
            Expression left = ParsePrimary(ref context);
            while (context.TryPeek(out Token opToken) && TryGetBinaryOp(opToken, out BinaryOperator op))
            {
                int precedence = GetPrecedence(op);
                if (precedence < minPrecedence)
                {
                    break;
                }

                context.Advance();
                Expression right = ParseExpression(ref context, precedence + 1);
                Range range = new(left.range.Start, right.range.End);
                left = new Binary(left, right, op, range, context.module);
            }

            return left;
        }

        private static bool TryGetBinaryOp(in Token token, out BinaryOperator op)
        {
            if (token.type == TokenType.Plus)
            {
                op = BinaryOperator.Add;
                return true;
            }
            else if (token.type == TokenType.Minus)
            {
                op = BinaryOperator.Subtract;
                return true;
            }
            else if (token.type == TokenType.Asterisk)
            {
                op = BinaryOperator.Multiply;
                return true;
            }
            else if (token.type == TokenType.Slash)
            {
                op = BinaryOperator.Divide;
                return true;
            }
            else if (token.type == TokenType.Percent)
            {
                op = BinaryOperator.Modulus;
                return true;
            }
            else if (token.type == TokenType.DoubleEquals)
            {
                op = BinaryOperator.Equal;
                return true;
            }
            else if (token.type == TokenType.NotEquals)
            {
                op = BinaryOperator.NotEqual;
                return true;
            }
            else if (token.type == TokenType.And)
            {
                op = BinaryOperator.And;
                return true;
            }
            else if (token.type == TokenType.Or)
            {
                op = BinaryOperator.Or;
                return true;
            }
            else
            {
                op = default;
                return false;
            }
        }

        private static int GetPrecedence(BinaryOperator op)
        {
            return op switch
            {
                BinaryOperator.Or => 1,
                BinaryOperator.And => 2,
                BinaryOperator.Equal or BinaryOperator.NotEqual => 3,
                BinaryOperator.Add or BinaryOperator.Subtract => 4,
                BinaryOperator.Multiply or BinaryOperator.Divide or BinaryOperator.Modulus => 5,
                _ => 0,
            };
        }

        private static Expression ParsePrimary(ref Context context)
        {
            int start = context.tokenIndex;

            // unary operators
            if (context.AdvanceIf(TokenType.Minus))
            {
                Expression operand = ParsePrimary(ref context);
                return new Unary(operand, UnaryOperator.Negate, context.GetRange(start), context.module);
            }

            if (context.AdvanceIf(TokenType.Bang))
            {
                Expression operand = ParsePrimary(ref context);
                return new Unary(operand, UnaryOperator.Not, context.GetRange(start), context.module);
            }

            // characters
            if (context.TryRead(TokenType.Character, out Token charToken))
            {
                ReadOnlySpan<char> raw = context.GetSourceCode(charToken);
                char value = raw[1];
                return new Character(value, charToken.range, context.module);
            }

            // strings
            if (context.TryRead(TokenType.Text, out Token textToken))
            {
                ReadOnlySpan<char> raw = context.GetSourceCode(textToken);
                string value = raw[1..^1].ToString();
                return new Text(value, textToken.range, context.module);
            }

            // numbers
            if (context.TryRead(TokenType.Number, out Token numToken))
            {
                ReadOnlySpan<char> raw = context.GetSourceCode(numToken);
                return new Number(raw.ToString(), numToken.range, context.module);
            }

            // keywords
            if (context.TryRead(TokenType.Keyword, out Token kwToken))
            {
                ReadOnlySpan<char> text = context.GetSourceCode(kwToken);
                if (text.SequenceEqual(KeywordMap.True))
                {
                    return ParsePostfix(ref context, new Boolean(true, kwToken.range, context.module), start);
                }
                else if (text.SequenceEqual(KeywordMap.False))
                {
                    return ParsePostfix(ref context, new Boolean(false, kwToken.range, context.module), start);
                }
                else if (text.SequenceEqual(KeywordMap.Null))
                {
                    return ParsePostfix(ref context, new NullLiteral(kwToken.range, context.module), start);
                }
                else if (text.SequenceEqual(KeywordMap.CreateInstance))
                {
                    if (!context.TryRead(TokenType.Keyword, out Token structNameToken))
                    {
                        throw new ExpectedNameOfTypeToConstruct(structNameToken, context.State);
                    }

                    string structName = context.GetSourceCode(structNameToken).ToString();
                    List<(string name, Expression value)> arguments = new();
                    if (context.AdvanceIf(TokenType.OpenParenthesis))
                    {
                        while (context.TryPeek(out Token next) && next.type != TokenType.CloseParenthesis)
                        {
                            if (arguments.Count > 0)
                            {
                                context.AdvanceIf(TokenType.Comma);
                            }

                            if (context.TryRead(TokenType.Keyword, out Token argNameToken) && context.AdvanceIf(TokenType.Equals))
                            {
                                string argName = context.GetSourceCode(argNameToken).ToString();
                                Expression argValue = ParseExpression(ref context);
                                arguments.Add((argName, argValue));
                            }
                            else
                            {
                                break;
                            }
                        }

                        context.AdvanceIf(TokenType.CloseParenthesis);
                    }

                    Expression result = new Construction(structName, arguments, context.GetRange(start), context.module);
                    return ParsePostfix(ref context, result, start);
                }

                Expression ident = new Identifier(text.ToString(), kwToken.range, context.module);
                return ParsePostfix(ref context, ident, start);
            }

            // parenthesized group
            if (context.TryPeek(out Token lastToken))
            {
                if (lastToken.type == TokenType.OpenParenthesis)
                {
                    context.Advance();
                    Expression inner = ParseExpression(ref context);
                    if (!context.TryPeek(out Token close) || close.type != TokenType.CloseParenthesis)
                    {
                        throw new ExpectedClosingParenthesis(context.State);
                    }

                    context.Advance();
                    return ParsePostfix(ref context, new Group(inner, context.GetRange(start), context.module), start);
                }
                else
                {
                    throw new ExpectedExpression(lastToken, context.State);
                }
            }
            else
            {
                throw new ExpectedTokens(context.State);
            }
        }

        private static Expression ParsePostfix(ref Context context, Expression expr, int start)
        {
            while (context.AdvanceIf(TokenType.Dot))
            {
                if (context.TryRead(TokenType.Keyword, out Token memberToken))
                {
                    string member = context.GetSourceCode(memberToken).ToString();
                    expr = new MemberAccess(expr, member, context.GetRange(start), context.module);
                }
            }

            return expr;
        }

        private static Statement ParseStatement(ref Context context)
        {
            int start = context.tokenIndex;
            if (context.TryPeek(TokenType.Keyword, out Token nextToken))
            {
                ReadOnlySpan<char> tokenText = context.GetSourceCode(nextToken);
                if (tokenText.SequenceEqual(KeywordMap.Return))
                {
                    context.Advance();
                    Expression value = null;
                    if (context.TryPeek(out Token afterReturn) && !IsTerminator(afterReturn.type))
                    {
                        value = ParseExpression(ref context);
                    }

                    return new Return(value, context.GetRange(start), context.module);
                }
                else if (tokenText.SequenceEqual(KeywordMap.If))
                {
                    context.Advance();
                    context.AdvanceIf(TokenType.OpenParenthesis);
                    Expression condition = ParseExpression(ref context);
                    context.AdvanceIf(TokenType.CloseParenthesis);
                    context.SkipNewlines();
                    Statement body = ParseStatement(ref context);
                    Statement elseBody = null;
                    context.SkipNewlines();
                    if (context.TryPeekKeyword(KeywordMap.Else))
                    {
                        context.Advance();
                        context.SkipNewlines();
                        elseBody = ParseStatement(ref context);
                    }

                    return new If(condition, body, elseBody, context.GetRange(start), context.module);
                }
                else if (tokenText.SequenceEqual(KeywordMap.TypeDeclaration))
                {
                    context.Advance();
                    if (context.TryRead(TokenType.Keyword, out Token structNameToken))
                    {
                        List<FieldDefinition> fields = new();
                        string structName = context.GetSourceCode(structNameToken).ToString();
                        context.SkipNewlines();
                        context.AdvanceIf(TokenType.OpenBrace);
                        context.SkipNewlines();

                        int fieldStart = context.tokenIndex;
                        while (context.TryPeekKeyword(KeywordMap.FieldDeclaration))
                        {
                            context.Advance();
                            if (context.TryRead(TokenType.Keyword, out Token fieldNameToken))
                            {
                                string fieldName = context.GetSourceCode(fieldNameToken).ToString();
                                FieldDefinition field = new(default, fieldName, context.GetRange(fieldStart), context.module);
                                fields.Add(field);
                                fieldStart = context.tokenIndex;
                            }

                            context.AdvanceIf(TokenType.Semicolon);
                            context.SkipNewlines();
                        }

                        context.AdvanceIf(TokenType.CloseBrace);
                        return new TypeDefinition(structName, fields.ToArray(), context.GetRange(start), context.module);
                    }
                    else
                    {
                        throw new ExpectedNameOfDeclaredType(structNameToken, context.State);
                    }
                }
                else if (tokenText.SequenceEqual(KeywordMap.FieldDeclaration))
                {
                    context.Advance();
                    if (context.TryRead(TokenType.Keyword, out Token nameToken))
                    {
                        Expression initializer = null;
                        if (context.AdvanceIf(TokenType.Equals))
                        {
                            initializer = ParseExpression(ref context);
                        }

                        string name = context.GetSourceCode(nameToken).ToString();
                        return new LocalVariable(name, initializer, context.GetRange(start), context.module);
                    }
                    else
                    {
                        throw new ExpectedNameOfVariable(context.State);
                    }
                }
            }

            if (context.IsNext(TokenType.Keyword) && context.TryPeekAt(1, TokenType.Dot))
            {
                int lookAhead = context.tokenIndex;
                lookAhead++;
                while (lookAhead + 1 < context.tokens.Count && context.tokens[lookAhead].type == TokenType.Dot && context.tokens[lookAhead + 1].type == TokenType.Keyword)
                {
                    lookAhead += 2;
                }

                if (lookAhead < context.tokens.Count && context.tokens[lookAhead].type == TokenType.Equals)
                {
                    int saved = context.tokenIndex;
                    Expression target = ParsePrimary(ref context);
                    if (target is MemberAccess access && context.AdvanceIf(TokenType.Equals))
                    {
                        Expression value = ParseExpression(ref context);
                        MemberAssignment memberAssignment = new(access.target, access.member, value, context.GetRange(start), context.module);
                        return new ExpressionStatement(memberAssignment, memberAssignment.range, context.module);
                    }

                    context.tokenIndex = saved;
                }
            }

            if (context.TryPeek(TokenType.Keyword, out Token idToken) && context.TryPeekAt(1, TokenType.Equals))
            {
                context.Advance();
                context.Advance();
                Expression value = ParseExpression(ref context);
                string name = context.GetSourceCode(idToken).ToString();
                Assignment assignment = new(name, value, context.GetRange(start), context.module);
                return new ExpressionStatement(assignment, assignment.range, context.module);
            }

            // block statement
            if (context.IsNext(TokenType.OpenBrace))
            {
                return ParseBlock(ref context);
            }

            Expression expr = ParseExpression(ref context);
            return new ExpressionStatement(expr, expr.range, context.module);
        }

        private static Block ParseBlock(ref Context context)
        {
            int start = context.tokenIndex;
            context.AdvanceIf(TokenType.OpenBrace);
            context.SkipNewlines();
            List<Statement> statements = new();
            while (context.TryPeek(out Token next) && next.type != TokenType.CloseBrace)
            {
                Statement statement = ParseStatement(ref context);
                statements.Add(statement);
                context.SkipNewlines();
            }

            context.AdvanceIf(TokenType.CloseBrace);
            Block block = new(context.GetRange(start), context.module);
            block.statements.AddRange(statements);
            return block;
        }

        public readonly ref struct State
        {
            public readonly int tokenIndex;
            public readonly ReadOnlySpan<char> sourceCode;
            public readonly List<Token> tokens;

            public State(int tokenIndex, ReadOnlySpan<char> sourceCode, List<Token> tokens)
            {
                this.tokenIndex = tokenIndex;
                this.sourceCode = sourceCode;
                this.tokens = tokens;
            }
        }

        private ref struct Context
        {
            public readonly Module module;
            public readonly ReadOnlySpan<char> sourceCode;
            public readonly List<Token> tokens;
            public int tokenIndex;

            public readonly State State => new(tokenIndex, sourceCode, tokens);
            public readonly bool AtEnd => tokenIndex >= tokens.Count;

            public Context(Module module, ReadOnlySpan<char> sourceCode, int tokenIndex)
            {
                this.module = module;
                this.sourceCode = sourceCode;
                this.tokenIndex = tokenIndex;
                tokens = new();
                Lexer.ReadAll(sourceCode, tokens);
            }

            public readonly ReadOnlySpan<char> GetSourceCode(Token token)
            {
                return token.GetSourceCode(sourceCode);
            }

            public readonly Range GetRange(int start)
            {
                Token startToken = tokens[start];
                Token endToken = tokens[tokenIndex - 1];
                Index startIndex = startToken.range.Start;
                Index endIndex = endToken.range.End;
                return new Range(startIndex, endIndex);
            }

            /// <summary>
            /// Tries to peek the <paramref name="nextToken"/>.
            /// </summary>
            /// <returns>
            /// <see langword="true"/> if there is a next token.
            /// </returns>
            public readonly bool TryPeek(out Token nextToken)
            {
                if (tokenIndex < tokens.Count)
                {
                    nextToken = tokens[tokenIndex];
                    return true;
                }
                else
                {
                    nextToken = default;
                    return false;
                }
            }

            /// <summary>
            /// Tries to peek the <paramref name="nextToken"/> assuming its type matches the given <paramref name="type"/>.
            /// </summary>
            /// <returns>
            /// <see langword="true"/> if there is a next token, and its type matches the given <paramref name="type"/>.
            /// </returns>
            public readonly bool TryPeek(TokenType type, out Token nextToken)
            {
                if (tokenIndex < tokens.Count && tokens[tokenIndex].type == type)
                {
                    nextToken = tokens[tokenIndex];
                    return true;
                }
                else
                {
                    nextToken = default;
                    return false;
                }
            }

            public void Advance()
            {
                tokenIndex++;
            }

            /// <summary>
            /// Advances the position only if the next token's type
            /// matches the <paramref name="expectedType"/>.
            /// </summary>
            public bool AdvanceIf(TokenType expectedType)
            {
                if (tokenIndex < tokens.Count && tokens[tokenIndex].type == expectedType)
                {
                    tokenIndex++;
                    return true;
                }

                return false;
            }

            public readonly bool IsNext(TokenType expectedType)
            {
                if (TryPeek(out Token token) && token.type == expectedType)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public bool TryRead(TokenType type, out Token token)
            {
                if (TryPeek(out token) && token.type == type)
                {
                    tokenIndex++;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public void SkipNewlines()
            {
                while (tokenIndex < tokens.Count && (tokens[tokenIndex].type == TokenType.NewLine || tokens[tokenIndex].type == TokenType.Semicolon))
                {
                    tokenIndex++;
                }
            }

            public void SkipTerminators()
            {
                SkipNewlines();
            }

            public readonly bool TryPeekAt(int offset, TokenType type)
            {
                int index = tokenIndex + offset;
                return index < tokens.Count && tokens[index].type == type;
            }

            public bool TryPeekKeyword(ReadOnlySpan<char> keyword)
            {
                if (TryPeek(TokenType.Keyword, out Token token))
                {
                    return token.GetSourceCode(sourceCode).SequenceEqual(keyword);
                }

                return false;
            }
        }
    }
}