namespace Scripting
{
    public class TypeValue
    {
        public readonly TypeSymbol symbol;
        public readonly Scope declaringScope;

        public TypeValue(TypeSymbol symbol, Scope declaringScope)
        {
            this.symbol = symbol;
            this.declaringScope = declaringScope;
        }
    }
}
