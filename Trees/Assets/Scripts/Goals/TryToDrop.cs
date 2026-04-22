using GOAP;

public class TryToDrop : Goal
{
    public TryToDrop()
    {
        Wants<ItemDropped>();
    }
}
