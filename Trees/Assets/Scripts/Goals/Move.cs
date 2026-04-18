using GOAP;
using UnityEngine;

public class Move : Goal
{
    public readonly Vector2 input;

    public Move(Vector2 input)
    {
        this.input = input;
        Wants<Moving>();
    }
}
