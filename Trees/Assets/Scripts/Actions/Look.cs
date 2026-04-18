using GOAP;

public class Look : Action
{
    public Look()
    {
        layer = 1;
        AddFact<LookingAt>();
    }
}
