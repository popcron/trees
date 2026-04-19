using System;
using UnityEngine;

public class CirclePacking
{
    public static readonly Vector2[][] knownSolutions;

    static CirclePacking()
    {
        knownSolutions = new Vector2[10][];
        knownSolutions[0] = new Vector2[] { };
        knownSolutions[1] = new Vector2[] { Vector2.zero };
        knownSolutions[2] = new Vector2[] { new Vector2(0.5f, 0f), new Vector2(-0.5f, 0f) };
        knownSolutions[3] = new Vector2[] { new Vector2(0f, 0.577f), new Vector2(-0.5f, -0.289f), new Vector2(0.5f, -0.289f) };
        knownSolutions[4] = new Vector2[] { new Vector2(0f, 0.707f), new Vector2(-0.707f, 0f), new Vector2(0f, -0.707f), new Vector2(0.707f, 0f) };
        knownSolutions[5] = new Vector2[] { new Vector2(0f, 0.809f), new Vector2(-0.809f, 0f), new Vector2(-0.25f, -0.769f), new Vector2(0.25f, -0.769f), new Vector2(0.809f, 0f) };
        knownSolutions[6] = new Vector2[] { new Vector2(0f, 0.866f), new Vector2(-0.866f, 0f), new Vector2(-0.5f, -0.289f), new Vector2(0.5f, -0.289f), new Vector2(-0.25f, -0.769f), new Vector2(0.25f, -0.769f) };
        knownSolutions[7] = new Vector2[] { new Vector2(0f, 1f), new Vector2(-1f, 0f), new Vector2(-0.707f, -0.707f), new Vector2(0f, -1f), new Vector2(0.707f, -0.707f), new Vector2(1f, 0f), new Vector2(0.707f, 0.707f) };
        knownSolutions[8] = new Vector2[] { new Vector2(0f, 1f), new Vector2(-0.866f, 0.5f), new Vector2(-0.866f, -0.5f), new Vector2(0f, -1f), new Vector2(0.866f, -0.5f), new Vector2(0.866f, 0.5f), new Vector2(-0.5f, 0.866f), new Vector2(-0.5f, -0.866f) };
        knownSolutions[9] = new Vector2[] { new Vector2(0f, 1f), new Vector2(-0.866f, 0.5f), new Vector2(-0.866f, -0.5f), new Vector2(0f, -1f), new Vector2(0.866f, -0.5f), new Vector2(0.866f, 0.5f), new Vector2(-0.5f, 0.866f), new Vector2(-0.5f, -0.866f), new Vector2(0.5f, 0.866f) };
    }

    private static float NextFloat()
    {
        return UnityEngine.Random.value;
    }

    public static void Generate(int count, float radius, Span<Vector2> points, int maxIterations = 512)
    {
        if (count < knownSolutions.Length)
        {
            ReadOnlySpan<Vector2> solution = knownSolutions[count];
            for (int i = 0; i < count; i++)
            {
                points[i] = solution[i] * radius;
            }

            return;
        }

        Span<Vector2> buffer = stackalloc Vector2[count];
        for (int i = 0; i < count; i++)
        {
            float angle = NextFloat() * Mathf.PI * 2;
            float r = Mathf.Sqrt(NextFloat()) * radius;
            buffer[i] = new Vector2(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r);
        }

        float radiusSquared = radius * radius;
        float minDistance = radius / Mathf.Sqrt(count);
        float minDistanceSquared = minDistance * minDistance;
        for (int r = 0; r < maxIterations; r++)
        {
            for (int a = 0; a < count; a++)
            {
                for (int b = a + 1; b < count; b++)
                {
                    Vector2 delta = buffer[b] - buffer[a];
                    float distanceSquared = delta.sqrMagnitude;
                    if (distanceSquared < 0.01f)
                    {
                        continue;
                    }

                    if (distanceSquared < minDistanceSquared)
                    {
                        float distance = Mathf.Sqrt(distanceSquared);
                        Vector2 push = delta / distance * (minDistance - distance) * 0.5f;
                        buffer[a] -= push;
                        buffer[b] += push;
                    }
                }
            }

            for (int i = 0; i < count; i++)
            {
                ref Vector2 point = ref buffer[i];
                float distanceSquared = point.sqrMagnitude;
                if (distanceSquared > radiusSquared)
                {
                    float distance = Mathf.Sqrt(distanceSquared);
                    point = (point / distance) * radius;
                }
            }
        }

        buffer.CopyTo(points);
    }
}