using System;

namespace Scripting
{
    public abstract class InterpreterException : Exception
    {
        public readonly Interpreter interpreter;
        public readonly Module module;
        public readonly Node node;

        public InterpreterException(Interpreter.State state)
        {
            interpreter = state.interpreter;
            module = state.module;
            node = state.node;
        }
    }
}