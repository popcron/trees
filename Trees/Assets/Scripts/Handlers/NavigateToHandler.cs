using GOAP;
using UnityEngine;

public class NavigateToHandler : ActionHandler<NavigateTo>
{
    public override bool TryComplete(Actor actor, Layer layer, Goal activeGoal, Action action, float delta)
    {
        if (activeGoal is TryToReach goal)
        {
            Unit unit = actor.GetComponent<Unit>();
            if (unit.Agent.TryResolve(unit.rigidbody, goal.destination, delta, out Vector2 move))
            {
                unit.actor.DispatchSubGoal(layer, new Move(move));
                if (!actor.layers[1].IsActive || actor.layers[1].TopGoal is LookAtNavigationTarget)
                {
                    Vector3 moveDirection = unit.rigidbody.linearVelocity.normalized;
                    if (moveDirection.sqrMagnitude > unit.maxSpeed * 0.3f)
                    {
                        moveDirection.y *= 0.5f;
                        Vector3 lookDestination = unit.head.position + moveDirection * 2f;
                        unit.actor.SubmitGoal(new LookAtNavigationTarget(lookDestination));
                    }
                }

                return false;
            }
        }

        return true;
    }
}
