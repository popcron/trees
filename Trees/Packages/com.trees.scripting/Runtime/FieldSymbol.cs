namespace Scripting
{
    public class FieldSymbol
    {
        public readonly Value.Type type;
        public readonly string name;
        public int index;

        public FieldSymbol(Value.Type type, string name)
        {
            this.type = type;
            this.name = name;
            index = -1;
        }

        public FieldSymbol(Value.Type type, string name, int index)
        {
            this.type = type;
            this.name = name;
            this.index = index;
        }

        public FieldSymbol Clone()
        {
            return new FieldSymbol(type, name, index);
        }
    }
}
