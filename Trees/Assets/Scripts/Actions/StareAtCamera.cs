using GOAP;

public class StareAtCamera : Action
{
    public StareAtCamera()
    {
        layer = 1;
        AddFact<LookingAtWithEyesOnly>();
    }
}
