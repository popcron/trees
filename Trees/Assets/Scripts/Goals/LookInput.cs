using GOAP;
using UnityEngine;

public class LookInput : Goal
{
    public readonly Vector2 look;

    public LookInput(Vector2 look)
    {
        this.look = look;
        Wants<LookingDirection>();
    }
}
