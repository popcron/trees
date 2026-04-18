namespace Scripting
{
    public class UnknownField : InterpreterException
    {
        public readonly TypeSymbol type;
        public readonly string member;

        public override string Message
        {
            get
            {
                // check for simple typos
                foreach (FieldSymbol fieldSymbol in type.fields)
                {
                    if (fieldSymbol.name.Equals(member, System.StringComparison.OrdinalIgnoreCase))
                    {
                        return $"Unknown field '{member}' in type {type.name} (did you mean {fieldSymbol.name}?)";
                    }
                }

                return $"Unknown field '{member}' in type {type.name}";
            }
        }

        public UnknownField(TypeSymbol type, string member, Interpreter.State state) : base(state)
        {
            this.type = type;
            this.member = member;
        }
    }
}