namespace Scripting
{
    public static class Opcode
    {
        public const int Nop = 0;
        public const int Pop = 1;

        public const int LoadNull = 2;
        public const int LoadTrue = 3;
        public const int LoadFalse = 4;
        public const int LoadLong = 5;
        public const int LoadDouble = 6;
        public const int LoadString = 7;
        public const int LoadCharacter = 8;

        public const int LoadLocal = 9;
        public const int StoreLocal = 10;
        public const int LoadBinding = 11;
        public const int StoreBinding = 12;

        public const int GetField = 13;
        public const int SetField = 14;

        public const int Binary = 15;
        public const int Unary = 16;

        public const int Jump = 17;
        public const int JumpIfFalse = 18;
        public const int JumpIfTrue = 19;

        public const int Call = 20;
        public const int Return = 21;
        public const int Construct = 22;

        public const int MakeFunction = 23;
        public const int MakeType = 24;
    }
}
