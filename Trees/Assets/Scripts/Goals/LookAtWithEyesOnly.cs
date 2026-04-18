using GOAP;
using UnityEngine;

public class LookAtWithEyesOnly : Goal
{
    public readonly Vector3 point;

    public LookAtWithEyesOnly(Vector3 point)
    {
        this.point = point;
        Wants<LookingAtWithEyesOnly>();
    }
}
