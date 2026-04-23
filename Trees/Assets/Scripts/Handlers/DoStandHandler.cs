using GOAP;

public class DoStandHandler : ActionHandler<DoStand>
{
    public override bool TryComplete(Actor actor, Layer layer, Goal activeGoal, Action action, float delta)
    {
        Unit unit = actor.GetComponent<Unit>();
        unit.ReleaseSeat();
        return true;
    }
}
