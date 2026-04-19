using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    private void Update()
    {
        Unit unit = Program.focalPoint as Unit;
        if (unit == null)
        {
            return;
        }

        PlayerInputState input = PlayerInputState.Get();
        unit.actor.SubmitGoal(new Strafe(input.movement));

        if (input.look != Vector2.zero)
        {
            unit.actor.SubmitGoal(new LookInput(input.look * 16f));
        }

        if (input.IsButtonPressed(PlayerInputState.Button.Jump))
        {
            unit.actor.SubmitGoal(new TryToJump());
        }
    }
}
