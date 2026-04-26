namespace Scripting
{
    public readonly struct Instruction
    {
        public readonly int opcode;
        public readonly int a;
        public readonly int b;

        public Instruction(int opcode)
        {
            this.opcode = opcode;
            a = 0;
            b = 0;
        }

        public Instruction(int opcode, int a)
        {
            this.opcode = opcode;
            this.a = a;
            b = 0;
        }

        public Instruction(int opcode, int a, int b)
        {
            this.opcode = opcode;
            this.a = a;
            this.b = b;
        }
    }
}
