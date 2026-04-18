using System;

namespace Scripting
{
    public abstract class Statement : Node
    {
        protected Statement(Range range, Module module) : base(range, module)
        {
        }
    }
}