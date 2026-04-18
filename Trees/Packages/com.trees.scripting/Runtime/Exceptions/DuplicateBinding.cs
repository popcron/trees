using System;

namespace Scripting
{
    public class DuplicateBinding : Exception
    {
        public readonly string name;

        public override string Message
        {
            get
            {
                return $"Constant with the name '{name}' already exists";
            }
        }

        public DuplicateBinding(string name)
        {
            this.name = name;
        }
    }
}