using System.Collections.Generic;
using System.Text;

public class StringBuilderPool
{
    private static readonly Stack<StringBuilder> stack = new();

    public static StringBuilderPool Shared { get; } = new();

    public StringBuilder Rent()
    {
        if (stack.TryPop(out StringBuilder stringBuilder))
        {
            return stringBuilder;
        }
        else
        {
            return new();
        }
    }

    public void Return(StringBuilder stringBuilder)
    {
        stringBuilder.Clear();
        stack.Push(stringBuilder);
    }

    public string ToStringAndReturn(StringBuilder stringBuilder)
    {
        string result = stringBuilder.ToString();
        stringBuilder.Clear();
        stack.Push(stringBuilder);
        return result;
    }
}
