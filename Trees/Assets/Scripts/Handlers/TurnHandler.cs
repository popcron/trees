using GOAP;
using UnityEngine;

public class TurnHandler : ActionHandler<Turn>
{
    private const float BodySpeed = 180f;
    private const float Threshold = 1f;

    public override bool TryComplete(Actor actor, Layer layer, Goal activeGoal, Action action, float delta)
    {
        FaceTowards goal = (FaceTowards)activeGoal;
        Unit unit = actor.GetComponent<Unit>();
        Vector3 toTarget = goal.direction - actor.transform.position;
        toTarget.y = 0f;
        float targetYaw = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;
        unit.bodyYaw = Mathf.MoveTowardsAngle(unit.bodyYaw, targetYaw, BodySpeed * delta);
        return Mathf.Abs(Mathf.DeltaAngle(unit.bodyYaw, targetYaw)) < Threshold;
    }
}
