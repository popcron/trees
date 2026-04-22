using UnityEngine;

public class Kart : MonoBehaviour
{
    public float gasSpeed = 8f;
    public float breakSpeed = 8f;
    public float steerSpeed = 42f;
    public float maxSteerAngle = 50f;
    public RaycastSpring[] drivingWheels = { };
    public RaycastSpring[] steeringWheels = { };
    public float gasInput = 0f;
    public float steerInput = 0f;
    public float breakInput = 0f;

    private void Update()
    {
        float delta = Time.deltaTime;
        PlayerInputState input = PlayerInputState.Get();
        float desiredSteerAngle = input.movement.x * maxSteerAngle;
        float desiredGasPedal = Mathf.Clamp01(input.movement.y);
        float desiredBreakPedal = Mathf.Clamp01(-input.movement.y);
        gasInput = Mathf.MoveTowards(gasInput, desiredGasPedal, gasSpeed * delta);
        breakInput = Mathf.MoveTowards(breakInput, desiredBreakPedal, breakSpeed * delta);
        steerInput = Mathf.MoveTowards(steerInput, desiredSteerAngle, steerSpeed * delta);
        for (int i = 0; i < drivingWheels.Length; i++)
        {
            drivingWheels[i].gasInput = gasInput;
            drivingWheels[i].brakeInput = breakInput;
        }

        for (int i = 0; i < steeringWheels.Length; i++)
        {
            steeringWheels[i].steeringAngle = steerInput;
            steeringWheels[i].brakeInput = breakInput;
        }
    }
}
