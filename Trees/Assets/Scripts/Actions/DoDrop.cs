using GOAP;

public class DoDrop : Action
{
    public DoDrop()
    {
        layer = 3;
        AddFact<ItemDropped>();
    }
}
