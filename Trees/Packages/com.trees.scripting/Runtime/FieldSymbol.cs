namespace Scripting
{
    public class FieldSymbol
    {
        public readonly Value.Type type;
        public readonly string name;

        public FieldSymbol(Value.Type type, string name)
        {
            this.type = type;
            this.name = name;
        }

        public FieldSymbol Clone()
        {
            return new FieldSymbol(type, name);
        }
    }
}
