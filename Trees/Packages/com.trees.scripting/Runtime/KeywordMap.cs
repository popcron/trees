using System;

namespace Scripting
{
    public static class KeywordMap
    {
        public const string True = "true";
        public const string False = "false";
        public const string Null = "null";
        public const string Return = "return";
        public const string If = "if";
        public const string Else = "else";
        public const string TypeDeclaration = "struct";
        public const string FieldDeclaration = "var";
        public const string VariableDeclaration = "var";
        public const string FunctionDeclaration = "fn";
        public const string CreateInstance = "new";

        public static readonly string[] AllKeywords = new[]
        {
            True,
            False,
            Null,
            Return,
            If,
            Else,
            TypeDeclaration,
            FieldDeclaration,
            VariableDeclaration,
            FunctionDeclaration,
            CreateInstance,
        };

        private static readonly ulong[] hashes = new ulong[AllKeywords.Length];

        static KeywordMap()
        {
            for (int i = 0; i < AllKeywords.Length; i++)
            {
                hashes[i] = ScriptingLibrary.GetHash(AllKeywords[i]);
            }
        }

        public static bool IsKeyword(ReadOnlySpan<char> word)
        {
            ulong hash = ScriptingLibrary.GetHash(word);
            return Array.IndexOf(hashes, hash) != -1;
        }
    }
}
