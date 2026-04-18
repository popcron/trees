using GOAP;

public class DoJump : Action
{
    public DoJump()
    {
        layer = 2;
        AddFact<InAir>();
    }
}