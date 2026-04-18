using GOAP;
using UnityEngine;

public class LookAt : Goal
{
    public readonly Vector3 point;

    public LookAt(Vector3 point)
    {
        this.point = point;
        Wants<LookingAt>();
    }
}
