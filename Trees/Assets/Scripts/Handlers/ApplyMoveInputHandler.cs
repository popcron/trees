using GOAP;
using UnityEngine;

public class ApplyMoveInputHandler : ActionHandler<ApplyMoveInput>
{
    public float lookAhead = 2f;

    public override bool TryComplete(Actor actor, Layer layer, Goal activeGoal, Action action, float delta)
    {
        if (activeGoal is Move move)
        {
            Unit unit = actor.GetComponent<Unit>();
            Vector3 origin = unit.agent.rigidbody.position;
            Vector3 direction = new(move.input.x, 0f, move.input.y);
            unit.agent.agent.SetDestination(origin + direction * lookAhead);
        }

        return true;
    }
}
