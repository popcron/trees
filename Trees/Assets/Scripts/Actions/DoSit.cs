using GOAP;

public class DoSit : Action
{
    public DoSit()
    {
        layer = 3;
        AddPreCondition<Standing>();
        AddFact<Seated>();
        RemoveFact<Standing>();
    }
}
