using GOAP;
using UnityEngine;
using UnityEngine.AI;

public class NavigateToHandler : ActionHandler<NavigateTo>
{
    public override bool TryComplete(Actor actor, Layer layer, Goal activeGoal, Action action, float delta)
    {
        if (activeGoal is TryToReach goal)
        {
            Unit unit = actor.GetComponent<Unit>();
            NavMeshAgent navAgent = unit.agent.agent;
            navAgent.SetDestination(goal.destination);
            if (navAgent.pathPending || navAgent.remainingDistance > navAgent.stoppingDistance)
            {
                if (!actor.layers[1].IsActive || actor.layers[1].TopGoal is LookAtNavigationTarget)
                {
                    Vector3 moveDirection = unit.agent.rigidbody.linearVelocity.normalized;
                    if (moveDirection.sqrMagnitude > unit.agent.agent.radius * unit.agent.agent.radius)
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
