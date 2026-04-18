using System;

namespace Scripting
{
    public class EnvironmentAlreadyExists : Exception
    {
        public readonly string label;

        public override string Message
        {
            get
            {
                return $"Globals with the label '{label}' already exists";
            }
        }

        public EnvironmentAlreadyExists(string label)
        {

        }
    }
}