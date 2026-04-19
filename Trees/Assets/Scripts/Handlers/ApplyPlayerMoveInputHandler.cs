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
            Vector3 worldMovement = Quaternion.Euler(0f, unit.bodyYaw, 0f) * localMovement;
            Vector2 move = new(worldMovement.x, worldMovement.z);
            unit.actor.DispatchSubGoal(layer, new Move(move));
            return true;
        }

        return true;
    }
}
