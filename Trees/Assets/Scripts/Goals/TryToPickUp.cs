using GOAP;

public class TryToPickUp : Goal
{
    public TryToPickUp()
    {
        Wants<ItemPickedUp>();
    }
}
