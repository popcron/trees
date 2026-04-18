using GOAP;
using UnityEngine;

public class LookAtCameraEyesOnlyHandler : ActionHandler<StareAtCamera>
{
    public override bool TryComplete(Actor actor, Layer layer, Goal activeGoal, Action action, float delta)
    {
        Unit unit = actor.GetComponent<Unit>();
        LookAtWithEyesOnly goal = (LookAtWithEyesOnly)activeGoal;
        Vector3 toTarget = goal.point - unit.head.position;
        Quaternion bodyRotation = Quaternion.Euler(0f, unit.bodyYaw, 0f);
        Quaternion headLocalRotation = Quaternion.Euler(unit.headPitchYaw.x, unit.headPitchYaw.y, 0f);
        Quaternion headWorldRotation = bodyRotation * headLocalRotation;
        Vector3 localToTarget = Quaternion.Inverse(headWorldRotation) * toTarget;
        float desiredEyeYaw = Mathf.Atan2(localToTarget.x, localToTarget.z) * Mathf.Rad2Deg;
        float localHorizontalDist = new Vector2(localToTarget.x, localToTarget.z).magnitude;
        float desiredEyePitch = -Mathf.Atan2(localToTarget.y, localHorizontalDist) * Mathf.Rad2Deg;
        unit.eyePitchYaw.y = desiredEyeYaw;
        unit.eyePitchYaw.x = desiredEyePitch;
        unit.eyeDistance = Vector3.Distance(unit.head.position, goal.point);
        return true;
    }
}
