using GOAP;

public class DoPickUp : Action
{
    public DoPickUp()
    {
        layer = 3;
        AddFact<ItemPickedUp>();
    }
}
