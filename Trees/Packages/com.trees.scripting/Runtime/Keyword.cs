using System;

namespace Scripting
{
    public readonly struct Keyword : IEquatable<Keyword>
    {
        public readonly ulong hash;

        public Keyword(ulong hash)
        {
            this.hash = hash;
        }

        public override string ToString()
        {
            return hash.ToString();
        }

        public readonly override bool Equals(object obj)
        {
            return obj is Keyword keyword && Equals(keyword);
        }

        public readonly bool Equals(Keyword other)
        {
            return hash == other.hash;
        }

        public readonly override int GetHashCode()
        {
            return hash.GetHashCode();
        }

        public static bool operator ==(Keyword left, Keyword right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Keyword left, Keyword right)
        {
            return !(left == right);
        }
    }
}
