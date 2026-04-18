using GOAP;

public class TryToJump : Goal
{
    public TryToJump()
    {
        Wants<InAir>();
    }
}