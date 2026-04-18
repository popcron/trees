using GOAP;
using UnityEngine;

public class Strafe : Goal
{
    public readonly Vector2 input;

    public Strafe(Vector2 input)
    {
        this.input = input;
        Wants<Strafing>();
    }
}
