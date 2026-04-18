using GOAP;
using UnityEngine;

public class LookAtNavigationTarget : Goal
{
    public readonly Vector3 point;

    public LookAtNavigationTarget(Vector3 point)
    {
        this.point = point;
        Wants<LookingAt>();
    }
}
