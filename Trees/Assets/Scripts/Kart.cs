using UI;
using UnityEngine;
using UnityEngine.InputSystem;

public class Kart : MonoBehaviour
{
    public Rigidbody carRigidbody;
    public WheelSettings settings;
    public Transform cameraAnchor;
    public Transform steeringWheel;
    public Vector3 centerOfMass = new(0f, -0.1f, 0f);
    public float gasSpeed = 8f;
    public float breakSpeed = 8f;
    public float steerSpeed = 42f;
    public float maxSteerAngle = 50f;
    public RaycastSpring[] drivingWheels = { };
    public RaycastSpring[] steeringWheels = { };
    public float smoothing = 3f;
    public float guiScale = 100f;
    public float smoothedWheelSpeed;
    public float smoothedCarSpeed;
    public float desiredSteerInput = 0f;
    public float desiredGasInput = 0f;
    public float desiredBreakInput = 0f;
    public float desiredHandbrakeInput = 0f;
    public Vector2 desiredPitchYaw = default;
    public bool desiredRespawn = false;
    public bool desiredReset = false;

    private WheelSettings activeSettings;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private float gasInput = 0f;
    private float steerInput = 0f;
    private float breakInput = 0f;
    private float handbrakeInput = 0f;
    private bool showTyreGUI = true;

    private void Awake()
    {
        originalPosition = carRigidbody.position;
        originalRotation = carRigidbody.rotation;
        activeSettings = Instantiate(settings);
    }

    private void OnEnable()
    {
        UIEngine.navigationEnabled = false;
    }

    private void OnDisable()
    {
        UIEngine.navigationEnabled = true;
    }

    private void Update()
    {
        if (desiredRespawn)
        {
            carRigidbody.linearVelocity = Vector3.zero;
            carRigidbody.angularVelocity = Vector3.zero;
            carRigidbody.position = originalPosition;
            carRigidbody.rotation = originalRotation;
            desiredPitchYaw = Vector2.zero;
        }

        if (desiredReset)
        {
            activeSettings = Instantiate(settings);
        }

        float delta = Time.deltaTime;
        gasInput = Mathf.MoveTowards(gasInput, desiredGasInput, gasSpeed * delta);
        breakInput = Mathf.MoveTowards(breakInput, desiredBreakInput, breakSpeed * delta);
        handbrakeInput = Mathf.MoveTowards(handbrakeInput, desiredHandbrakeInput, breakSpeed * delta);
        steerInput = Mathf.MoveTowards(steerInput, desiredSteerInput * maxSteerAngle, steerSpeed * delta);
        for (int i = 0; i < drivingWheels.Length; i++)
        {
            drivingWheels[i].gasInput = gasInput;
            drivingWheels[i].brakeInput = breakInput;
            drivingWheels[i].handbrakeInput = handbrakeInput;
            drivingWheels[i].settings = activeSettings;
        }

        for (int i = 0; i < steeringWheels.Length; i++)
        {
            steeringWheels[i].steeringAngle = steerInput;
            steeringWheels[i].brakeInput = breakInput;
            steeringWheels[i].handbrakeInput = 0f;
            steeringWheels[i].settings = activeSettings;
        }

        desiredPitchYaw.x = Mathf.Clamp(desiredPitchYaw.x, -89, 89);
        cameraAnchor.localEulerAngles = new Vector3(desiredPitchYaw.x, desiredPitchYaw.y, 0f);
        steeringWheel.localEulerAngles = new Vector3(0f, steerInput * Mathf.PI * 2f, 0f);

        if (Keyboard.current != null && Keyboard.current.f2Key.wasPressedThisFrame)
        {
            showTyreGUI = !showTyreGUI;
        }

        IMUILayout.Label("T = factory settings");
        IMUILayout.Label("R/Xbox Y/Triangle = respawn");
        IMUILayout.Label("Space/Xbox B/Circle = Handbrake");
        IMUILayout.Label("F2 = Hide/Show Tyre GUI");
        IMUILayout.Space(50f);
        SettingsGUI();

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

        Rect carRect = new(Screen.width - 300f, Screen.height - 60f, 300f, 60f);
        using (IMUILayout.BeginArea(carRect))
        {
            CarGUI();
        }

        if (showTyreGUI)
        {
            Vector2 tyrePanelSize = new(200f, 120f);
            for (int i = 0; i < steeringWheels.Length; i++)
            {
                RaycastSpring sw = steeringWheels[i];
                if (!IMUI.Project(sw.transform.position, tyrePanelSize, null, out Rect tyreRect))
                {
                    continue;
                }

                using IMUILayout.Scope tyreArea = IMUILayout.BeginArea(tyreRect, i);
                TyreGUI(sw);
            }

            for (int i = 0; i < drivingWheels.Length; i++)
            {
                RaycastSpring dw = drivingWheels[i];
                if (!IMUI.Project(dw.transform.position, tyrePanelSize, null, out Rect tyreRect))
                {
                    continue;
                }

                using IMUILayout.Scope tyreArea = IMUILayout.BeginArea(tyreRect, i + steeringWheels.Length);
                TyreGUI(dw);
            }
        }

        float pedalWidth = 30f;
        float pedalHeight = 70f;
        float steerWidth = 200f;
        float panelWidth = steerWidth;
        float panelHeight = 220f;
        Rect inputArea = new(Screen.width * 0.5f - panelWidth * 0.5f, Screen.height - panelHeight, panelWidth, panelHeight);
        using (IMUILayout.BeginArea(inputArea))
        {
            SteerBar(desiredSteerInput, steerWidth, pedalHeight, new Color(0.3f, 0.65f, 1f));
            IMUILayout.Space(8f);
            using (IMUILayout.BeginHorizontal(panelWidth))
            {
                IMUILayout.FlexibleSpace();
                Pedal(desiredGasInput, pedalWidth, pedalHeight, new Color(0.25f, 0.85f, 0.3f));
                IMUILayout.Space(8f);
                Pedal(desiredBreakInput, pedalWidth, pedalHeight, new Color(0.9f, 0.25f, 0.25f));
                IMUILayout.Space(8f);
                Pedal(desiredHandbrakeInput, pedalWidth, pedalHeight, new Color(0.95f, 0.7f, 0.15f));
                IMUILayout.FlexibleSpace();
            }
        }
    }

    private void SettingsGUI()
    {
        SliderRow("Max Steer Angle: ", ref maxSteerAngle, 0f, 180f, "F1");
        SliderRow("Center of Mass Y: ", ref centerOfMass.y, -1f, 1f, "F2");
        SliderRow("Center of Mass Z: ", ref centerOfMass.z, -1f, 1f, "F2");
        SliderRow("Brake Torque: ", ref activeSettings.brakeTorque, 0f, 100f, "F2");
        SliderRow("Damping Ratio: ", ref activeSettings.dampingRatio, 0f, 1.5f, "F2");
        SliderRow("Peak Friction μ: ", ref activeSettings.peakFrictionCoefficient, 0.2f, 2f, "F2");
        SliderRow("Lateral Stiffness: ", ref activeSettings.lateralStiffness, 100f, 3000f, "F0");
        SliderRow("Longitudinal Stiffness: ", ref activeSettings.longitudinalStiffness, 10f, 300f, "F0");
        SliderRow("Front ARB: ", ref activeSettings.frontAntiRollStiffness, 0f, 15000f, "F0");
        SliderRow("Rear ARB: ", ref activeSettings.rearAntiRollStiffness, 0f, 15000f, "F0");
        SliderRow("Forward Torque Scale: ", ref activeSettings.engineTorqueScale, 0f, 100f, "F2");
        SliderRow("Reverse Torque Scale: ", ref activeSettings.reverseTorqueScale, 0f, 100f, "F2");
        SliderRow("Forward Top Speed: ", ref activeSettings.forwardTopSpeed, 0f, 100f, "F2");
        SliderRow("Reverse Top Speed: ", ref activeSettings.reverseTopSpeed, 0f, 100f, "F2");
    }

    private static void SliderRow(string label, ref float value, float min, float max, string format, [System.Runtime.CompilerServices.CallerLineNumber] int line = 0)
    {
        using (IMUILayout.BeginHorizontal(400f, key: line))
        {
            IMUILayout.Label(label, 200f);
            IMUILayout.Label(value.ToString(format), 100f);
            IMUILayout.HorizontalSlider(ref value, min, max, 200f);
        }
    }

    private void FixedUpdate()
    {
        carRigidbody.centerOfMass = centerOfMass;
        carRigidbody.mass = activeSettings.mass;
        ApplyAntiRollBar(steeringWheels, activeSettings.frontAntiRollStiffness);
        ApplyAntiRollBar(drivingWheels, activeSettings.rearAntiRollStiffness);
    }

    private void ApplyAntiRollBar(RaycastSpring[] axle, float stiffness)
    {
        if (axle.Length < 2 || stiffness <= 0f)
        {
            return;
        }

        RaycastSpring a = axle[0];
        RaycastSpring b = axle[1];
        if (!a.grounded || !b.grounded)
        {
            return;
        }

        float difference = a.currentCompression - b.currentCompression;
        float force = difference * stiffness;
        carRigidbody.AddForceAtPosition(a.currentSpringDirection * force, a.currentSpringOrigin);
        carRigidbody.AddForceAtPosition(-b.currentSpringDirection * force, b.currentSpringOrigin);
    }

    private void OnDrawGizmos()
    {
        EditorGizmos.color = Color.red;
        Vector3 worldCenterOfMass = carRigidbody.position + centerOfMass;
        EditorGizmos.DrawLine(carRigidbody.position, worldCenterOfMass, 3f);
    }

    public void CarGUI()
    {
        IMUILayout.Label($"Wheel Surface Speed: {smoothedWheelSpeed * 3.6f:F1} km/h");
        IMUILayout.Label($"Car Speed: {smoothedCarSpeed * 3.6f:F1} km/h");
    }

    public void TyreGUI(RaycastSpring spring)
    {
        IMUILayout.Label($"Lon Slip: {spring.normalizedLongitudinalSlip:F2}");
        IMUILayout.Label($"Lat Slip: {spring.normalizedLateralSlip:F2}");
        IMUILayout.Label($"Sus Distance: {spring.suspensionRestDistance:F2}");
        IMUILayout.HorizontalSlider(ref spring.suspensionRestDistance, 0.05f, 1f);
    }

    private static void Pedal(float value, float width, float height, Color color, [System.Runtime.CompilerServices.CallerLineNumber] int line = 0)
    {
        float fill = height * Mathf.Clamp01(value);
        Color trackColor = new(0.12f, 0.12f, 0.12f, 0.65f);
        using (IMUILayout.BeginVertical(width, key: line))
        {
            using (IMUILayout.BeginVertical(width, height, key: line))
            {
                IMUILayout.Box(trackColor, width, height - fill, key: line);
                IMUILayout.Box(color, width, fill, key: line);
            }
        }
    }

    private static void SteerBar(float value, float width, float height, Color color, [System.Runtime.CompilerServices.CallerLineNumber] int line = 0)
    {
        float half = width * 0.5f;
        float clamped = Mathf.Clamp(value, -1f, 1f);
        float leftFill = clamped < 0f ? -clamped * half : 0f;
        float rightFill = clamped > 0f ? clamped * half : 0f;
        float barHeight = 18f;
        float topPad = (height - barHeight) * 0.5f;
        Color trackColor = new(0.12f, 0.12f, 0.12f, 0.65f);
        Color tickColor = new(1f, 1f, 1f, 0.85f);
        using (IMUILayout.BeginVertical(width, key: line))
        {
            IMUILayout.Space(topPad, key: line);
            using (IMUILayout.BeginHorizontal(width, barHeight, key: line))
            {
                IMUILayout.Box(trackColor, half - leftFill, barHeight, key: line);
                IMUILayout.Box(color, leftFill, barHeight, key: line);
                IMUILayout.Box(tickColor, 2f, barHeight, key: line);
                IMUILayout.Box(color, rightFill, barHeight, key: line);
                IMUILayout.Box(trackColor, half - rightFill, barHeight, key: line);
            }
        }
    }
}
