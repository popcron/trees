using GOAP;

public class ApplyLookInput : Action
{
    public ApplyLookInput()
    {
        layer = 1;
        AddFact<LookingDirection>();
    }
}