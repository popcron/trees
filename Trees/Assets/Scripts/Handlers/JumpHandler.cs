using GOAP;
using UnityEngine;

public class JumpHandler : ActionHandler<DoJump>
{
    public override bool TryComplete(Actor actor, Layer layer, Goal activeGoal, Action action, float delta)
    {
        if (activeGoal is TryToJump)
        {
            Unit unit = actor.GetComponent<Unit>();
            if (unit.grounded)
            {
                unit.grounded = false;
                unit.groundCheckCooldown = 0.25f;
                float force = Mathf.Sqrt(2 * unit.maxJumpHeight * Physics.gravity.magnitude);
                unit.agent.rigidbody.linearVelocity += Vector3.up * force;
                return true;
            }
        }

        return false;
    }
}