using GOAP;

public class JumpHandler : ActionHandler<DoJump>
{
    public override bool TryComplete(Actor actor, Layer layer, Goal activeGoal, Action action, float delta)
    {
        if (activeGoal is TryToJump)
        {
            Unit unit = actor.GetComponent<Unit>();
            if (unit.grounded)
            {
                unit.jump = true;
                return true;
            }
        }

        return false;
    }
}