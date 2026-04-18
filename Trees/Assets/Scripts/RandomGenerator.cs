using UnityEngine;

public struct RandomGenerator
{
    public ulong state;

    public RandomGenerator(ulong state)
    {
        this.state = state;
    }

    public RandomGenerator(int state)
    {
        this.state = (ulong)state;
    }

    public ulong NextULong()
    {
        state ^= state << 13;
        state ^= state >> 7;
        state ^= state << 17;
        return state;
    }

    public int NextInt()
    {
        state ^= state << 13;
        state ^= state >> 7;
        state ^= state << 17;
        return (int)state;
    }

    public int NextInt(int min, int max)
    {
        return (int)(NextULong() % (ulong)(max - min)) + min;
    }

    public double NextDouble()
    {
        state ^= state << 13;
        state ^= state >> 7;
        state ^= state << 17;
        return state / (double)ulong.MaxValue;
    }

    public float NextFloat()
    {
        state ^= state << 13;
        state ^= state >> 7;
        state ^= state << 17;
        return state / (float)ulong.MaxValue;
    }

    public float NextFloat(float min, float max)
    {
        return NextFloat() * (max - min) + min;
    }

    public Vector2 NextUnitCircle()
    {
        float angle = NextFloat() * Mathf.PI * 2f;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }
}