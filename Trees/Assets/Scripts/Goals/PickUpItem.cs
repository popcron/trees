using GOAP;
using UnityEngine;

public class PickUpItem : Goal
{
    public readonly Rigidbody target;

    public PickUpItem(Rigidbody target)
    {
        this.target = target;
        Wants<ItemPickedUp>();
    }
}
