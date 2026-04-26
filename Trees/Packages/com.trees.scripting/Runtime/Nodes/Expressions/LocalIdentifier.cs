using System;

namespace Scripting
{
    public class LocalIdentifier : Identifier
    {
        public readonly int frameDepth;
        public readonly int slotIndex;

        public LocalIdentifier(Identifier source, int frameDepth, int slotIndex) : base(source.value, source.range, source.module)
        {
            this.frameDepth = frameDepth;
            this.slotIndex = slotIndex;
        }
    }
}
