using System;

namespace Scripting
{
    public abstract class Literal : Expression
    {
        protected Literal(Range range, Module file) : base(range, file)
        {
        }
    }
}