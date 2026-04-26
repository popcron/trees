namespace Scripting
{
    public class Scope
    {
        public readonly Scope parent;
        public readonly Value[] slots;

        public Scope(Scope parent, int slotCount = 0)
        {
            this.parent = parent;
            slots = new Value[slotCount];
        }

        public Scope Ancestor(int depth)
        {
            Scope scope = this;
            for (int i = 0; i < depth; i++)
            {
                scope = scope.parent;
            }

            return scope;
        }
    }
}
