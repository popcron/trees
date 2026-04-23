using UnityEngine;

public static class SitProbe
{
    public const float ClearanceRadius = 0.5f;
    public const float ProbeDistance = 1.25f;
    public const float MaxSupportTilt = 40f;

    private static readonly Collider[] overlapBuffer = new Collider[16];

    public static bool TryFind(Vector3 origin, Vector3 up, Vector3 forward, LayerMask supportMask, LayerMask clearanceMask, Transform ignoreRoot, out SitCandidate candidate)
    {
        candidate = default;
        Ray ray = new(origin + up * ClearanceRadius, -up);
        int hitCount = Physics.RaycastNonAlloc(ray, raycastBuffer, ProbeDistance + ClearanceRadius, supportMask, QueryTriggerInteraction.Ignore);
        RaycastHit hit = default;
        float closest = float.MaxValue;
        bool found = false;
        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit candidateHit = raycastBuffer[i];
            if (ignoreRoot != null && candidateHit.collider.transform.IsChildOf(ignoreRoot))
            {
                continue;
            }

            if (candidateHit.distance < closest)
            {
                closest = candidateHit.distance;
                hit = candidateHit;
                found = true;
            }
        }

        if (!found)
        {
            return false;
        }

        if (Vector3.Angle(hit.normal, up) > MaxSupportTilt)
        {
            return false;
        }

        Vector3 seat = hit.point + hit.normal * ClearanceRadius;
        int overlap = Physics.OverlapSphereNonAlloc(seat, ClearanceRadius, overlapBuffer, clearanceMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < overlap; i++)
        {
            Collider other = overlapBuffer[i];
            if (other == hit.collider)
            {
                continue;
            }

            if (ignoreRoot != null && other.transform.IsChildOf(ignoreRoot))
            {
                continue;
            }

            return false;
        }

        Vector3 flatForward = Vector3.ProjectOnPlane(forward, hit.normal);
        if (flatForward.sqrMagnitude < 0.0001f)
        {
            flatForward = Vector3.ProjectOnPlane(Vector3.forward, hit.normal);
        }

        candidate.position = seat;
        candidate.rotation = Quaternion.LookRotation(flatForward.normalized, hit.normal);
        candidate.supportNormal = hit.normal;
        candidate.support = hit.collider.attachedRigidbody;
        candidate.supportCollider = hit.collider;
        return true;
    }

    private static readonly RaycastHit[] raycastBuffer = new RaycastHit[16];
}
