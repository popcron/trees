using System;
using System.Collections.Generic;

namespace Scripting
{
    public static class Parser
    {
        public static Module Parse(ReadOnlySpan<char> sourceCode, ReadOnly readOnly)
        {
            Module module = new(readOnly);
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
            while (context.TryPeek(out Token opToken))
            {
                BinaryOperator op;
                int consumed;
                if (context.TryPeekAdjacent(out Token second))
                {
                    char a = (char)opToken.type;
                    char b = (char)second.type;
                    if (a == '=' && b == '=')
                    {
                        op = BinaryOperator.Equal;
                        consumed = 2;
                    }
                    else if (a == '!' && b == '=')
                    {
                        op = BinaryOperator.NotEqual;
                        consumed = 2;
                    }
                    else if (a == '&' && b == '&')
                    {
                        op = BinaryOperator.And;
                        consumed = 2;
                    }
                    else if (a == '|' && b == '|')
                    {
                        op = BinaryOperator.Or;
                        consumed = 2;
                    }
                    else if (a == '>' && b == '=')
                    {
                        op = BinaryOperator.GreaterEqual;
                        consumed = 2;
                    }
                    else if (a == '<' && b == '=')
                    {
                        op = BinaryOperator.LessEqual;
                        consumed = 2;
                    }
                    else if (TryGetBinaryOp(opToken, out op))
                    {
                        consumed = 1;
                    }
                    else
                    {
                        break;
                    }
                }
                else if (TryGetBinaryOp(opToken, out op))
                {
                    consumed = 1;
                }
                else
                {
                    break;
                }

                int precedence = GetPrecedence(op);
                if (precedence < minPrecedence)
                {
                    break;
                }

                for (int i = 0; i < consumed; i++)
                {
                    context.Advance();
                }

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
            else if ((char)token.type == '>')
            {
                op = BinaryOperator.Greater;
                return true;
            }
            else if ((char)token.type == '<')
            {
                op = BinaryOperator.Less;
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
                BinaryOperator.Greater or BinaryOperator.Less or BinaryOperator.GreaterEqual or BinaryOperator.LessEqual => 4,
                BinaryOperator.Add or BinaryOperator.Subtract => 5,
                BinaryOperator.Multiply or BinaryOperator.Divide or BinaryOperator.Modulus => 6,
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
                return new Number(raw, numToken.range, context.module);
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
                    Expression typeExpression = ParseTypeExpression(ref context);
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

                    Expression result = new Construction(typeExpression, arguments, context.GetRange(start), context.module);
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
                        throw new ExpectedClosingParenthesis(close, context.State);
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
            while (true)
            {
                if (context.AdvanceIf(TokenType.Dot))
                {
                    if (context.TryRead(TokenType.Keyword, out Token memberToken))
                    {
                        string member = context.GetSourceCode(memberToken).ToString();
                        expr = new MemberAccess(expr, member, context.GetRange(start), context.module);
                    }
                }
                else if (context.AdvanceIf(TokenType.OpenParenthesis))
                {
                    List<Expression> arguments = new();
                    while (context.TryPeek(out Token next) && next.type != TokenType.CloseParenthesis)
                    {
                        if (arguments.Count > 0)
                        {
                            context.AdvanceIf(TokenType.Comma);
                        }

                        Expression argument = ParseExpression(ref context);
                        arguments.Add(argument);
                    }

                    context.AdvanceIf(TokenType.CloseParenthesis);
                    expr = new Call(expr, arguments, context.GetRange(start), context.module);
                }
                else
                {
                    break;
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
                    return ParseTypeDefinition(ref context);
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
                else if (tokenText.SequenceEqual(KeywordMap.FunctionDeclaration))
                {
                    return ParseFunctionDefinition(ref context);
                }
            }

            if (context.LooksLikeMemberAssignment())
            {
                int saved = context.tokenIndex;
                Expression target = ParsePrimary(ref context);
                if (target is MemberAccess access)
                {
                    if (context.AdvanceIf(TokenType.Equals))
                    {
                        Expression value = ParseExpression(ref context);
                        MemberAssignment memberAssignment = new(access.target, access.member, value, context.GetRange(start), context.module);
                        return new ExpressionStatement(memberAssignment, memberAssignment.range, context.module);
                    }

                    if (context.TryReadCompoundAssign(out BinaryOperator compoundOp))
                    {
                        Expression rhs = ParseExpression(ref context);
                        MemberAccess read = new(access.target, access.member, access.range, context.module);
                        Range valueRange = new(access.range.Start, rhs.range.End);
                        Binary combined = new(read, rhs, compoundOp, valueRange, context.module);
                        MemberAssignment memberAssignment = new(access.target, access.member, combined, context.GetRange(start), context.module);
                        return new ExpressionStatement(memberAssignment, memberAssignment.range, context.module);
                    }
                }

                context.tokenIndex = saved;
            }

            if (context.TryPeek(TokenType.Keyword, out Token idToken))
            {
                if (context.TryPeekAt(1, TokenType.Equals))
                {
                    context.Advance();
                    context.Advance();
                    Expression value = ParseExpression(ref context);
                    string name = context.GetSourceCode(idToken).ToString();
                    Assignment assignment = new(name, value, context.GetRange(start), context.module);
                    return new ExpressionStatement(assignment, assignment.range, context.module);
                }

                if (context.TryPeekCompoundAssignAt(1, out BinaryOperator compoundOp))
                {
                    context.Advance();
                    context.TryReadCompoundAssign(out _);
                    Expression rhs = ParseExpression(ref context);
                    string name = context.GetSourceCode(idToken).ToString();
                    Identifier read = new(name, idToken.range, context.module);
                    Range valueRange = new(idToken.range.Start, rhs.range.End);
                    Binary combined = new(read, rhs, compoundOp, valueRange, context.module);
                    Assignment assignment = new(name, combined, context.GetRange(start), context.module);
                    return new ExpressionStatement(assignment, assignment.range, context.module);
                }
            }

            // block statement
            if (context.IsNext(TokenType.OpenBrace))
            {
                return ParseBlock(ref context);
            }

            Expression expr = ParseExpression(ref context);
            return new ExpressionStatement(expr, expr.range, context.module);
        }

        private static Expression ParseTypeExpression(ref Context context)
        {
            int start = context.tokenIndex;
            if (!context.TryRead(TokenType.Keyword, out Token nameToken))
            {
                throw new ExpectedNameOfTypeToConstruct(nameToken, context.State);
            }

            string name = context.GetSourceCode(nameToken).ToString();
            Expression expr = new Identifier(name, nameToken.range, context.module);
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

        private static TypeDefinition ParseTypeDefinition(ref Context context)
        {
            int start = context.tokenIndex;
            context.Advance(); // consume 'struct'
            if (!context.TryRead(TokenType.Keyword, out Token structNameToken))
            {
                throw new ExpectedNameOfDeclaredType(structNameToken, context.State);
            }

            List<FieldDefinition> fields = new();
            List<FunctionDefinition> methods = new();
            List<TypeDefinition> types = new();
            string structName = context.GetSourceCode(structNameToken).ToString();
            context.SkipNewlines();
            context.AdvanceIf(TokenType.OpenBrace);
            context.SkipNewlines();

            int memberStart = context.tokenIndex;
            while (context.TryPeek(TokenType.Keyword, out Token memberToken))
            {
                ReadOnlySpan<char> memberText = context.GetSourceCode(memberToken);
                if (memberText.SequenceEqual(KeywordMap.FieldDeclaration))
                {
                    context.Advance();
                    if (context.TryRead(TokenType.Keyword, out Token fieldNameToken))
                    {
                        string fieldName = context.GetSourceCode(fieldNameToken).ToString();
                        FieldDefinition field = new(default, fieldName, context.GetRange(memberStart), context.module);
                        fields.Add(field);
                    }
                }
                else if (memberText.SequenceEqual(KeywordMap.FunctionDeclaration))
                {
                    methods.Add(ParseFunctionDefinition(ref context));
                }
                else if (memberText.SequenceEqual(KeywordMap.TypeDeclaration))
                {
                    types.Add(ParseTypeDefinition(ref context));
                }
                else
                {
                    break;
                }

                context.AdvanceIf(TokenType.Semicolon);
                context.SkipNewlines();
                memberStart = context.tokenIndex;
            }

            context.AdvanceIf(TokenType.CloseBrace);
            return new TypeDefinition(structName, fields.ToArray(), methods.ToArray(), types.ToArray(), context.GetRange(start), context.module);
        }

        private static FunctionDefinition ParseFunctionDefinition(ref Context context)
        {
            int start = context.tokenIndex;
            context.Advance(); // consume 'fn'
            if (!context.TryRead(TokenType.Keyword, out Token nameToken))
            {
                throw new ExpectedNameOfFunction(context.State);
            }

            string fnName = context.GetSourceCode(nameToken).ToString();
            List<string> parameters = new();
            context.AdvanceIf(TokenType.OpenParenthesis);
            while (context.TryPeek(out Token next) && next.type != TokenType.CloseParenthesis)
            {
                if (parameters.Count > 0)
                {
                    context.AdvanceIf(TokenType.Comma);
                }

                if (context.TryRead(TokenType.Keyword, out Token paramKeyword))
                {
                    ReadOnlySpan<char> paramKeywordText = context.GetSourceCode(paramKeyword);
                    if (!paramKeywordText.SequenceEqual(KeywordMap.VariableDeclaration))
                    {
                        throw new ExpectedParameterDeclaration(paramKeyword, context.State);
                    }

                    if (!context.TryRead(TokenType.Keyword, out Token paramNameToken))
                    {
                        throw new ExpectedNameOfParameter(context.State);
                    }

                    parameters.Add(context.GetSourceCode(paramNameToken).ToString());
                }
                else
                {
                    break;
                }
            }

            context.AdvanceIf(TokenType.CloseParenthesis);
            context.SkipNewlines();
            Block body = ParseBlock(ref context);
            return new FunctionDefinition(fnName, parameters.ToArray(), body, context.GetRange(start), context.module);
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

            public readonly bool TryPeekAdjacent(out Token next)
            {
                int index = tokenIndex + 1;
                if (index < tokens.Count && tokens[tokenIndex].range.End.Value == tokens[index].range.Start.Value)
                {
                    next = tokens[index];
                    return true;
                }

                next = default;
                return false;
            }

            public readonly bool TryPeekAt(int offset, TokenType type)
            {
                int index = tokenIndex + offset;
                return index < tokens.Count && tokens[index].type == type;
            }

            public readonly bool LooksLikeMemberAssignment()
            {
                if (!IsNext(TokenType.Keyword) || !TryPeekAt(1, TokenType.Dot))
                {
                    return false;
                }

                int lookAhead = tokenIndex + 1;
                while (lookAhead + 1 < tokens.Count && tokens[lookAhead].type == TokenType.Dot && tokens[lookAhead + 1].type == TokenType.Keyword)
                {
                    lookAhead += 2;
                }

                if (lookAhead >= tokens.Count)
                {
                    return false;
                }

                if (tokens[lookAhead].type == TokenType.Equals)
                {
                    return true;
                }

                return TryPeekCompoundAssignAt(lookAhead - tokenIndex, out _);
            }

            public readonly bool TryPeekCompoundAssignAt(int offset, out BinaryOperator op)
            {
                int index = tokenIndex + offset;
                if (index + 1 >= tokens.Count)
                {
                    op = default;
                    return false;
                }

                Token first = tokens[index];
                Token second = tokens[index + 1];
                if (first.range.End.Value != second.range.Start.Value || second.type != TokenType.Equals)
                {
                    op = default;
                    return false;
                }

                if (first.type == TokenType.Plus)
                {
                    op = BinaryOperator.Add;
                    return true;
                }

                if (first.type == TokenType.Minus)
                {
                    op = BinaryOperator.Subtract;
                    return true;
                }

                if (first.type == TokenType.Asterisk)
                {
                    op = BinaryOperator.Multiply;
                    return true;
                }

                if (first.type == TokenType.Slash)
                {
                    op = BinaryOperator.Divide;
                    return true;
                }

                if (first.type == TokenType.Percent)
                {
                    op = BinaryOperator.Modulus;
                    return true;
                }

                op = default;
                return false;
            }

            public bool TryReadCompoundAssign(out BinaryOperator op)
            {
                if (TryPeekCompoundAssignAt(0, out op))
                {
                    tokenIndex += 2;
                    return true;
                }

                return false;
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