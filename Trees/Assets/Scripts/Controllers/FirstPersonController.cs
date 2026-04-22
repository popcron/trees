using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public PlayerInputState input;

    private bool lastFirstAction;
    private bool lastSecondAction;

    private void OnEnable()
    {
        input = PlayerInputState.Get();
        lastFirstAction = input.IsButtonPressed(PlayerInputState.Button.FirstAction);
        lastSecondAction = input.IsButtonPressed(PlayerInputState.Button.SecondAction);
    }

    private void Update()
    {
        input = PlayerInputState.Get();
        Unit unit = Program.focalPoint as Unit;
        if (unit == null)
        {
            return;
        }

        unit.actor.SubmitGoal(new Strafe(input.movement));

        if (input.look != Vector2.zero)
        {
            unit.actor.SubmitGoal(new LookInput(input.look * 16f));
        }

        if (input.IsButtonPressed(PlayerInputState.Button.Jump))
        {
            unit.actor.SubmitGoal(new TryToJump());
        }

        if (lastFirstAction != input.IsButtonPressed(PlayerInputState.Button.FirstAction))
        {
            lastFirstAction = !lastFirstAction;
            if (lastFirstAction)
            {
                unit.actor.SubmitGoal(new TryToPickUp());
            }
        }

        if (lastSecondAction != input.IsButtonPressed(PlayerInputState.Button.SecondAction))
        {
            lastSecondAction = !lastSecondAction;
            if (lastSecondAction)
            {
                unit.actor.SubmitGoal(new TryToDrop());
            }
        }
    }
}
