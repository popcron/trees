using System;
using System.Text;

namespace Scripting
{
    public class Number : Literal
    {
        public readonly bool isDouble;
        public readonly long longValue;
        public readonly double doubleValue;

        public Number(ReadOnlySpan<char> value, Range range, Module file) : base(range, file)
        {
            if (long.TryParse(value, out long parsedLong))
            {
                longValue = parsedLong;
                isDouble = false;
            }
            else
            {
                doubleValue = double.Parse(value);
                isDouble = true;
            }
        }

        public override void Append(StringBuilder stringBuilder, int depth)
        {
            if (isDouble)
            {
                stringBuilder.Append(doubleValue);
            }
            else
            {
                stringBuilder.Append(longValue);
            }
        }
    }
}
