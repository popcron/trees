using System;

namespace Scripting
{
    public abstract class Expression : Node
    {
        /// <summary>
        /// The statement or expression that owns this expression.
        /// </summary>
        public Node Parent => module.GetParentOf(this);

        protected Expression(Range range, Module file) : base(range, file)
        {
        }
    }
}