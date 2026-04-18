using GOAP;

public class ApplyMoveInputHandler : ActionHandler<ApplyMoveInput>
{
    public override bool TryComplete(Actor actor, Layer layer, Goal activeGoal, Action action, float delta)
    {
        if (activeGoal is Move move)
        {
            Unit unit = actor.GetComponent<Unit>();
            unit.input = move.input;
        }

        return true;
    }
}
