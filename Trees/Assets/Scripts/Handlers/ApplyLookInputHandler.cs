using GOAP;
using UnityEngine;

public class ApplyLookInputHandler : ActionHandler<ApplyLookInput>
{
    public float lookSensitivity = 0.1f;

    public override bool TryComplete(Actor actor, Layer layer, Goal activeGoal, Action action, float delta)
    {
        if (activeGoal is LookInput lookInput)
        {
            Unit unit = actor.GetComponent<Unit>();
            unit.bodyYaw += lookInput.look.x * lookSensitivity;
            unit.headPitchYaw.x = Mathf.Clamp(unit.headPitchYaw.x - lookInput.look.y * lookSensitivity, -89.9f, 89.9f);
            unit.eyePitchYaw = Vector2.zero;
        }

        return true;
    }
}
