using GOAP;
using UnityEngine;

public class ApplyPlayerMoveInputHandler : ActionHandler<ApplyStrafeInput>
{
    public override bool TryComplete(Actor actor, Layer layer, Goal activeGoal, Action action, float delta)
    {
        if (activeGoal is Strafe strafe)
        {
            Unit unit = actor.GetComponent<Unit>();
            Vector3 localMovement = new(strafe.input.x, 0f, strafe.input.y);
            Vector3 bodyRotation = Quaternion.Euler(0f, unit.bodyYaw, 0f) * localMovement;
            unit.input = new Vector2(bodyRotation.x, bodyRotation.z);
        }

        return true;
    }
}
