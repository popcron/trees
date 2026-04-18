using GOAP;
using UnityEngine;

public class FaceTowards : Goal
{
    public readonly Vector3 direction;

    public FaceTowards(Vector3 direction)
    {
        this.direction = direction;
        Wants<FacingDirection>();
    }
}
