namespace Scripting
{
    public class FunctionValue
    {
        public readonly FunctionSymbol symbol;
        public readonly Scope closure;
        public readonly Value boundSelf;

        public FunctionValue(FunctionSymbol symbol, Scope closure)
        {
            this.symbol = symbol;
            this.closure = closure;
            boundSelf = Value.Null;
        }

        public FunctionValue(FunctionSymbol symbol, Scope closure, Value boundSelf)
        {
            this.symbol = symbol;
            this.closure = closure;
            this.boundSelf = boundSelf;
        }
    }
}
