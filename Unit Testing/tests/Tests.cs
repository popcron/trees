using System;

namespace Scripting.Tests;

public abstract class Tests
{
    protected static readonly Interpreter interpreter = new();

    protected static Value Evaluate(ReadOnlySpan<char> sourceCode)
    {
        return interpreter.Evaluate(sourceCode);
    }
}
