using System;

namespace Scripting
{
    public class BindingIdentifier : Identifier
    {
        public readonly int bindingIndex;

        public BindingIdentifier(Identifier source, int bindingIndex) : base(source.value, source.range, source.module)
        {
            this.bindingIndex = bindingIndex;
        }
    }
}
