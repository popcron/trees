using GOAP;
using UnityEngine;

public class LookHandler : ActionHandler<Look>
{
    private const float EyeSpeed = 720f;
    private const float HeadSpeed = 360f;
    private const float BodySpeed = 180f;
    private const float Threshold = 1f;

    public override bool TryComplete(Actor actor, Layer layer, Goal activeGoal, Action action, float delta)
    {
        Vector3 lookPoint = activeGoal switch
        {
            LookAt goal => goal.point,
            LookAtNavigationTarget goal => goal.point,
            _ => Vector3.zero
        };

        Unit unit = actor.GetComponent<Unit>();
        Vector3 toTarget = lookPoint - unit.head.position;
        float desiredYaw = Mathf.Atan2(toTarget.x, toTarget.z) * Mathf.Rad2Deg;
        float horizontalDist = new Vector2(toTarget.x, toTarget.z).magnitude;
        float desiredPitch = -Mathf.Atan2(toTarget.y, horizontalDist) * Mathf.Rad2Deg;
        float targetBodyYaw = desiredYaw;
        unit.bodyYaw = Mathf.MoveTowardsAngle(unit.bodyYaw, targetBodyYaw, BodySpeed * delta);
        float remainingYaw = Mathf.DeltaAngle(unit.bodyYaw, desiredYaw);
        float targetHeadYaw = Mathf.Clamp(remainingYaw, -60f, 60f);
        unit.headPitchYaw.y = Mathf.MoveTowardsAngle(unit.headPitchYaw.y, targetHeadYaw, HeadSpeed * delta);
        unit.headPitchYaw.x = Mathf.MoveTowards(unit.headPitchYaw.x, desiredPitch, HeadSpeed * delta);
        Quaternion bodyRotation = Quaternion.Euler(0f, unit.bodyYaw, 0f);
        Quaternion headLocalRotation = Quaternion.Euler(unit.headPitchYaw.x, unit.headPitchYaw.y, 0f);
        Quaternion headWorldRotation = bodyRotation * headLocalRotation;
        Vector3 localToTarget = Quaternion.Inverse(headWorldRotation) * toTarget;
        float desiredEyeYaw = Mathf.Atan2(localToTarget.x, localToTarget.z) * Mathf.Rad2Deg;
        float localHorizontalDist = new Vector2(localToTarget.x, localToTarget.z).magnitude;
        float desiredEyePitch = -Mathf.Atan2(localToTarget.y, localHorizontalDist) * Mathf.Rad2Deg;
        unit.eyePitchYaw.y = Mathf.MoveTowardsAngle(unit.eyePitchYaw.y, desiredEyeYaw, EyeSpeed * delta);
        unit.eyePitchYaw.x = Mathf.MoveTowards(unit.eyePitchYaw.x, desiredEyePitch, EyeSpeed * delta);
        unit.eyeDistance = Vector3.Distance(unit.head.position, lookPoint);
        bool bodyDone = Mathf.Abs(Mathf.DeltaAngle(unit.bodyYaw, targetBodyYaw)) < Threshold;
        bool headPitchDone = Mathf.Abs(unit.headPitchYaw.x - desiredPitch) < Threshold;
        bool headYawDone = Mathf.Abs(Mathf.DeltaAngle(unit.headPitchYaw.y, targetHeadYaw)) < Threshold && headPitchDone;
        return bodyDone && headYawDone && headPitchDone;
    }
}
