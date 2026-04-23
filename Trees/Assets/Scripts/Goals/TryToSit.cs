using GOAP;

public class TryToSit : Goal
{
    public TryToSit()
    {
        Wants<Seated>();
    }
}
