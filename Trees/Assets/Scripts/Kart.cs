using UnityEngine;

public class Kart : MonoBehaviour
{
    private static GUIStyle labelStyle;

    public Rigidbody carRigidbody;
    public Vector3 centerOfMass = new(0f, -0.1f, 0f);
    public float gasSpeed = 8f;
    public float breakSpeed = 8f;
    public float steerSpeed = 42f;
    public float maxSteerAngle = 50f;
    public RaycastSpring[] drivingWheels = { };
    public RaycastSpring[] steeringWheels = { };
    public float gasInput = 0f;
    public float steerInput = 0f;
    public float breakInput = 0f;
    public float smoothing = 3f;
    public float guiScale = 100f;
    private float smoothedWheelSpeed;
    private float smoothedCarSpeed;

    private void Update()
    {
        float delta = Time.deltaTime;
        PlayerInputState input = PlayerInputState.Get();
        float desiredSteerAngle = input.movement.x * maxSteerAngle;
        float desiredGasPedal = Mathf.Clamp01(input.actions.y);
        float desiredBreakPedal = Mathf.Clamp01(input.actions.x);
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

    private void FixedUpdate()
    {
        carRigidbody.centerOfMass = centerOfMass;
    }

    private void OnDrawGizmos()
    {
        EditorGizmos.color = Color.red;
        Vector3 worldCenterOfMass = carRigidbody.position + centerOfMass;
        EditorGizmos.DrawLine(carRigidbody.position, worldCenterOfMass, 3f);
    }

    private void OnGUI()
    {
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 26,
                fontStyle = FontStyle.Normal,
            };
        }

        float averageWheelAngularVelocity = 0f;
        float averageWheelRadius = 0f;
        for (int i = 0; i < drivingWheels.Length; i++)
        {
            averageWheelAngularVelocity += drivingWheels[i].wheelAngularVelocity;
            averageWheelRadius += drivingWheels[i].wheelRadius;
        }

        averageWheelAngularVelocity /= drivingWheels.Length;
        averageWheelRadius /= drivingWheels.Length;
        float wheelSurfaceSpeed = averageWheelAngularVelocity * averageWheelRadius;
        float carSpeed = Vector3.Dot(carRigidbody.transform.forward, carRigidbody.linearVelocity);
        float t = 1f - Mathf.Exp(-smoothing * Time.deltaTime);
        smoothedWheelSpeed = Mathf.Lerp(smoothedWheelSpeed, wheelSurfaceSpeed, t);
        smoothedCarSpeed = Mathf.Lerp(smoothedCarSpeed, carSpeed, t);

        float centerX = Screen.width * 0.5f;
        float centerY = Screen.height * 0.5f;
        Rect carGui = new(centerX - 400, centerY - 200f, 700f, 700f);
        GUILayout.BeginArea(carGui);
        CarGUI();
        GUILayout.EndArea();

        Rect tyresGui = new(centerX + 400, centerY - 200f, 700f, 700f);
        for (int i = 0; i < steeringWheels.Length; i++)
        {
            RaycastSpring steeringWheel = steeringWheels[i];
            GUILayout.BeginArea(new Rect(tyresGui.x + steeringWheel.transform.localPosition.x * guiScale, tyresGui.y + steeringWheel.transform.localPosition.z * guiScale, tyresGui.width, tyresGui.height));
            TyreGUI(steeringWheel);
            GUILayout.EndArea();
        }

        for (int i = 0; i < drivingWheels.Length; i++)
        {
            RaycastSpring drivingWheel = drivingWheels[i];
            GUILayout.BeginArea(new Rect(tyresGui.x + drivingWheel.transform.localPosition.x * guiScale, tyresGui.y + drivingWheel.transform.localPosition.z * guiScale, tyresGui.width, tyresGui.height));
            TyreGUI(drivingWheel);
            GUILayout.EndArea();
        }

        PlayerInputState input = PlayerInputState.Get();
        Rect playerInputArea = new(centerX - 200f, Screen.height - 200f, 400f, 200f);
        GUILayout.BeginArea(playerInputArea);
        PlayerInputGUI(input);
        GUILayout.EndArea();
    }

    public void CarGUI()
    {
        GUILayout.Label($"Wheel Surface Speed: {smoothedWheelSpeed * 3.6f:F1} km/h", labelStyle);
        GUILayout.Label($"Car Speed: {smoothedCarSpeed * 3.6f:F1} km/h", labelStyle);
    }

    public void TyreGUI(RaycastSpring spring)
    {
        GUILayout.Label($"Lon Slip: {spring.normalizedLongitudinalSlip:F2}", labelStyle);
        GUILayout.Label($"Lat Slip: {spring.normalizedLateralSlip:F2}", labelStyle);
    }

    public void PlayerInputGUI(PlayerInputState input)
    {
        int maxBars = 30;
        int throttleBars = Mathf.RoundToInt(input.actions.y * maxBars);
        GUILayout.Label($"T: {new string('|', throttleBars)}", labelStyle);

        int brakeBars = Mathf.RoundToInt(input.actions.x * maxBars);
        GUILayout.Label($"B: {new string('|', brakeBars)}", labelStyle);

        GUILayout.BeginHorizontal(GUILayout.Width(400));
        {
            GUILayout.Label("S: ", labelStyle, GUILayout.Width(30));
            GUIStyle leftStyle = new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleRight };
            int steerLeftBars = input.movement.x < 0 ? Mathf.RoundToInt(-input.movement.x * (maxBars * 0.5f)) : 0;
            GUILayout.Label(new string('|', steerLeftBars), leftStyle, GUILayout.Width(100));
            GUILayout.Label("|", labelStyle, GUILayout.Width(10));
            GUIStyle rightStyle = new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleLeft };
            int steerRightBars = input.movement.x > 0 ? Mathf.RoundToInt(input.movement.x * (maxBars * 0.5f)) : 0;
            GUILayout.Label(new string('|', steerRightBars), rightStyle, GUILayout.Width(100));
        }
        GUILayout.EndHorizontal();
    }
}
