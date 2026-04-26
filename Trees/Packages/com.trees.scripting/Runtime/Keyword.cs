using System;

namespace Scripting
{
    public readonly struct Keyword : IEquatable<Keyword>
    {
        public readonly int id;

        public Keyword(int id)
        {
            this.id = id;
        }

        public override string ToString()
        {
            return id.ToString();
        }

        public readonly override bool Equals(object obj)
        {
            return obj is Keyword keyword && Equals(keyword);
        }

        public readonly bool Equals(Keyword other)
        {
            return id == other.id;
        }

        public readonly override int GetHashCode()
        {
            return id.GetHashCode();
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
