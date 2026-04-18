using System;

namespace Scripting
{
    public abstract class Definition : Statement
    {
        protected Definition(Range range, Module module) : base(range, module)
        {
        }
    }
}