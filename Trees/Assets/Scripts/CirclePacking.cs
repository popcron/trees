using System;
using UnityEngine;

public class CirclePacking
{
    private static float NextFloat()
    {
        return UnityEngine.Random.value;
    }

    public static void Generate(int count, float radius, Span<Vector2> points, int maxIterations = 512)
    {
        if (count == 0)
        {
            return;
        }
        else if (count == 1)
        {
            points[0] = Vector2.zero;
            return;
        }
        else if (count == 2)
        {
            points[0] = new Vector2(radius, radius);
            points[1] = new Vector2(-radius, -radius);
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