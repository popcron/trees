using GOAP;
using UnityEngine;

public class TryToReach : Goal
{
    public readonly Vector3 destination;

    public TryToReach(Vector3 destination)
    {
        this.destination = destination;
        Wants<AtDestination>();
    }
}
