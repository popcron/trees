using UnityEngine;

public static class Raycasting
{
    private static readonly RaycastHit[] results = new RaycastHit[32];

    public static bool TryGetClosestHit(Ray ray, float distance, out RaycastHit closestHit)
    {
        return TryGetClosestHit(ray, distance, ~0, out closestHit);
    }

    public static bool TryGetClosestHit(Ray ray, float distance, int layerMask, out RaycastHit closestHit)
    {
        closestHit = default;
        float closestDistance = float.MaxValue;
        int count = Physics.RaycastNonAlloc(ray, results, distance, layerMask);
        for (int i = 0; i < count; i++)
        {
            RaycastHit currentHit = results[i];
            float currentDistance = currentHit.distance;
            if (currentDistance < closestDistance)
            {
                closestDistance = currentDistance;
                closestHit = currentHit;
            }
        }

        return closestDistance < float.MaxValue;
    }

    public static bool TryGetClosestHit(Ray ray, float distance, SelectCallback select, out RaycastHit closestHit)
    {
        return TryGetClosestHit(ray, distance, ~0, select, out closestHit);
    }

    public static bool TryGetClosestHit(Ray ray, float distance, int layerMask, SelectCallback select, out RaycastHit closestHit)
    {
        closestHit = default;
        float closestDistance = float.MaxValue;
        int count = Physics.RaycastNonAlloc(ray, results, distance, layerMask);
        for (int i = 0; i < count; i++)
        {
            RaycastHit currentHit = results[i];
            float currentDistance = currentHit.distance;
            if (currentDistance < closestDistance)
            {
                closestDistance = currentDistance;
                if (select(currentHit))
                {
                    closestHit = currentHit;
                }
            }
        }

        return closestHit.collider != null;
    }

    public delegate bool SelectCallback(RaycastHit hit);
}