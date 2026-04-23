using GOAP;

public class DoStand : Action
{
    public DoStand()
    {
        layer = 3;
        AddPreCondition<Seated>();
        AddFact<Standing>();
        RemoveFact<Seated>();
    }
}
