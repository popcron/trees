using UnityEngine;
using UnityEngine.InputSystem;

public class Kart : MonoBehaviour
{
    private static GUIStyle labelStyle;

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
    public float gasInput = 0f;
    public float steerInput = 0f;
    public float breakInput = 0f;
    public float handbrakeInput = 0f;
    public float smoothing = 3f;
    public float guiScale = 100f;
    public float smoothedWheelSpeed;
    public float smoothedCarSpeed;
    public Vector3 originalPosition;
    public Quaternion originalRotation;
    public float desiredSteerInput = 0f;
    public float desiredGasInput = 0f;
    public float desiredBreakInput = 0f;
    public float desiredHandbrakeInput = 0f;
    public Vector2 desiredPitchYaw = default;
    public bool desiredResetCar = false;
    public bool desiredResetSettings = false;

    private WheelSettings activeSettings;

    private void Awake()
    {
        originalPosition = carRigidbody.position;
        originalRotation = carRigidbody.rotation;
        activeSettings = Instantiate(settings);
    }

    private void Update()
    {
        desiredSteerInput = 0f;
        desiredGasInput = 0f;
        desiredBreakInput = 0f;
        desiredHandbrakeInput = 0f;
        desiredResetCar = false;
        desiredResetSettings = false;
        if (Keyboard.current is Keyboard keyboard)
        {
            if (keyboard.aKey.isPressed)
            {
                desiredSteerInput -= 1f;
            }

            if (keyboard.dKey.isPressed)
            {
                desiredSteerInput += 1f;
            }

            if (keyboard.wKey.isPressed)
            {
                desiredGasInput += 1f;
            }

            if (keyboard.sKey.isPressed)
            {
                desiredBreakInput += 1f;
            }

            if (keyboard.spaceKey.isPressed)
            {
                desiredHandbrakeInput = 1f;
            }

            if (keyboard.rKey.isPressed)
            {
                desiredResetCar = true;
            }

            if (keyboard.tKey.isPressed)
            {
                desiredResetSettings = true;
            }
        }

        if (Mouse.current is Mouse mouse)
        {
            //desiredPitchYaw.x += mouse.delta.y.ReadValue() * 0.1f;
            //desiredPitchYaw.y += mouse.delta.x.ReadValue() * 0.1f;
        }

        if (Gamepad.current is Gamepad gamepad)
        {
            desiredSteerInput += gamepad.leftStick.x.ReadValue();
            desiredBreakInput += gamepad.leftTrigger.ReadValue();
            desiredGasInput += gamepad.rightTrigger.ReadValue();
            desiredPitchYaw.x += gamepad.rightStick.y.ReadValue() * 2f;
            desiredPitchYaw.y += gamepad.rightStick.x.ReadValue() * 2f;
            if (gamepad.buttonEast.isPressed)
            {
                desiredHandbrakeInput = 1f;
            }

            if (gamepad.buttonNorth.wasPressedThisFrame)
            {
                desiredResetCar = true;
            }
        }

        if (desiredResetCar)
        {
            carRigidbody.linearVelocity = Vector3.zero;
            carRigidbody.angularVelocity = Vector3.zero;
            carRigidbody.position = originalPosition;
            carRigidbody.rotation = originalRotation;
            desiredPitchYaw = Vector2.zero;
        }

        if (desiredResetSettings)
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

    private void OnGUI()
    {
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 16,
                fontStyle = FontStyle.Normal,
            };
        }

        GUILayout.Label("T = factory settings");
        GUILayout.Label("R/Xbox Y/Triangle = respawn");
        GUILayout.Label("Space/Xbox B/Circle = Handbrake");

        // max steer angle slider
        GUILayout.BeginHorizontal(GUILayout.Width(400));
        {
            GUILayout.Label("Max Steer Angle: ", labelStyle, GUILayout.Width(200));
            GUILayout.Label($"{maxSteerAngle:F1}", labelStyle, GUILayout.Width(100));
            maxSteerAngle = GUILayout.HorizontalSlider(maxSteerAngle, 0f, 180f, GUILayout.Width(200));
        }
        GUILayout.EndHorizontal();

        // center of mass slider (-1, 1)
        GUILayout.BeginHorizontal(GUILayout.Width(400));
        {
            GUILayout.Label("Center of Mass Y: ", labelStyle, GUILayout.Width(200));
            GUILayout.Label($"{centerOfMass.y:F2}", labelStyle, GUILayout.Width(100));
            centerOfMass.y = GUILayout.HorizontalSlider(centerOfMass.y, -1f, 1f, GUILayout.Width(200));
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal(GUILayout.Width(400));
        {
            GUILayout.Label("Center of Mass Z: ", labelStyle, GUILayout.Width(200));
            GUILayout.Label($"{centerOfMass.z:F2}", labelStyle, GUILayout.Width(100));
            centerOfMass.z = GUILayout.HorizontalSlider(centerOfMass.z, -1f, 1f, GUILayout.Width(200));
        }
        GUILayout.EndHorizontal();

        // brake torque
        GUILayout.BeginHorizontal(GUILayout.Width(400));
        {
            GUILayout.Label("Brake Torque: ", labelStyle, GUILayout.Width(200));
            GUILayout.Label($"{activeSettings.brakeTorque:F2}", labelStyle, GUILayout.Width(100));
            activeSettings.brakeTorque = GUILayout.HorizontalSlider(activeSettings.brakeTorque, 0f, 100f, GUILayout.Width(200));
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal(GUILayout.Width(400));
        {
            GUILayout.Label("Damping Ratio: ", labelStyle, GUILayout.Width(200));
            GUILayout.Label($"{activeSettings.dampingRatio:F2}", labelStyle, GUILayout.Width(100));
            activeSettings.dampingRatio = GUILayout.HorizontalSlider(activeSettings.dampingRatio, 0f, 1.5f, GUILayout.Width(200));
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal(GUILayout.Width(400));
        {
            GUILayout.Label("Peak Friction μ: ", labelStyle, GUILayout.Width(200));
            GUILayout.Label($"{activeSettings.peakFrictionCoefficient:F2}", labelStyle, GUILayout.Width(100));
            activeSettings.peakFrictionCoefficient = GUILayout.HorizontalSlider(activeSettings.peakFrictionCoefficient, 0.2f, 2f, GUILayout.Width(200));
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal(GUILayout.Width(400));
        {
            GUILayout.Label("Lateral Stiffness: ", labelStyle, GUILayout.Width(200));
            GUILayout.Label($"{activeSettings.lateralStiffness:F0}", labelStyle, GUILayout.Width(100));
            activeSettings.lateralStiffness = GUILayout.HorizontalSlider(activeSettings.lateralStiffness, 100f, 3000f, GUILayout.Width(200));
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal(GUILayout.Width(400));
        {
            GUILayout.Label("Longitudinal Stiffness: ", labelStyle, GUILayout.Width(200));
            GUILayout.Label($"{activeSettings.longitudinalStiffness:F0}", labelStyle, GUILayout.Width(100));
            activeSettings.longitudinalStiffness = GUILayout.HorizontalSlider(activeSettings.longitudinalStiffness, 10f, 300f, GUILayout.Width(200));
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal(GUILayout.Width(400));
        {
            GUILayout.Label("Front ARB: ", labelStyle, GUILayout.Width(200));
            GUILayout.Label($"{activeSettings.frontAntiRollStiffness:F0}", labelStyle, GUILayout.Width(100));
            activeSettings.frontAntiRollStiffness = GUILayout.HorizontalSlider(activeSettings.frontAntiRollStiffness, 0f, 15000f, GUILayout.Width(200));
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal(GUILayout.Width(400));
        {
            GUILayout.Label("Rear ARB: ", labelStyle, GUILayout.Width(200));
            GUILayout.Label($"{activeSettings.rearAntiRollStiffness:F0}", labelStyle, GUILayout.Width(100));
            activeSettings.rearAntiRollStiffness = GUILayout.HorizontalSlider(activeSettings.rearAntiRollStiffness, 0f, 15000f, GUILayout.Width(200));
        }
        GUILayout.EndHorizontal();

        // forward torque scale
        GUILayout.BeginHorizontal(GUILayout.Width(400));
        {
            GUILayout.Label("Forward Torque Scale: ", labelStyle, GUILayout.Width(200));
            GUILayout.Label($"{activeSettings.engineTorqueScale:F2}", labelStyle, GUILayout.Width(100));
            activeSettings.engineTorqueScale = GUILayout.HorizontalSlider(activeSettings.engineTorqueScale, 0f, 100f, GUILayout.Width(200));
        }
        GUILayout.EndHorizontal();

        // reverse torque scale
        GUILayout.BeginHorizontal(GUILayout.Width(400));
        {
            GUILayout.Label("Reverse Torque Scale: ", labelStyle, GUILayout.Width(200));
            GUILayout.Label($"{activeSettings.reverseTorqueScale:F2}", labelStyle, GUILayout.Width(100));
            activeSettings.reverseTorqueScale = GUILayout.HorizontalSlider(activeSettings.reverseTorqueScale, 0f, 100f, GUILayout.Width(200));
        }
        GUILayout.EndHorizontal();

        // forward top speed
        GUILayout.BeginHorizontal(GUILayout.Width(400));
        {
            GUILayout.Label("Forward Top Speed: ", labelStyle, GUILayout.Width(200));
            GUILayout.Label($"{activeSettings.forwardTopSpeed:F2}", labelStyle, GUILayout.Width(100));
            activeSettings.forwardTopSpeed = GUILayout.HorizontalSlider(activeSettings.forwardTopSpeed, 0f, 100f, GUILayout.Width(200));
        }
        GUILayout.EndHorizontal();

        // reverse top speed
        GUILayout.BeginHorizontal(GUILayout.Width(400));
        {
            GUILayout.Label("Reverse Top Speed: ", labelStyle, GUILayout.Width(200));
            GUILayout.Label($"{activeSettings.reverseTopSpeed:F2}", labelStyle, GUILayout.Width(100));
            activeSettings.reverseTopSpeed = GUILayout.HorizontalSlider(activeSettings.reverseTopSpeed, 0f, 100f, GUILayout.Width(200));
        }
        GUILayout.EndHorizontal();

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
        Rect carGui = new(Screen.width - 300, Screen.height - 60f, 300f, 60);
        GUILayout.BeginArea(carGui);
        CarGUI();
        GUILayout.EndArea();

        Rect tyresGui = new(centerX + 200, centerY - 200f, 700f, 700f);
        for (int i = 0; i < steeringWheels.Length; i++)
        {
            RaycastSpring steeringWheel = steeringWheels[i];
            GUILayout.BeginArea(new Rect(tyresGui.x + steeringWheel.transform.localPosition.x * guiScale * 1.4f, tyresGui.y + steeringWheel.transform.localPosition.z * guiScale, tyresGui.width, tyresGui.height));
            TyreGUI(steeringWheel);
            GUILayout.EndArea();
        }

        for (int i = 0; i < drivingWheels.Length; i++)
        {
            RaycastSpring drivingWheel = drivingWheels[i];
            GUILayout.BeginArea(new Rect(tyresGui.x + drivingWheel.transform.localPosition.x * guiScale * 1.4f, tyresGui.y + drivingWheel.transform.localPosition.z * guiScale, tyresGui.width, tyresGui.height));
            TyreGUI(drivingWheel);
            GUILayout.EndArea();
        }

        Rect playerInputArea = new(200f, Screen.height - 100f, 400f, 100f);
        GUILayout.BeginArea(playerInputArea);
        PlayerInputGUI();
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
        GUILayout.Label($"Sus Distance: {spring.suspensionRestDistance:F2}", labelStyle, GUILayout.Width(100));
        spring.suspensionRestDistance = GUILayout.HorizontalSlider(spring.suspensionRestDistance, 0.05f, 1f, GUILayout.Width(160));
    }

    public void PlayerInputGUI()
    {
        int maxBars = 30;
        GUILayout.BeginHorizontal(GUILayout.Width(400));
        {
            int throttleBars = Mathf.RoundToInt(desiredGasInput * maxBars);
            GUILayout.Label($"T: {new string('|', throttleBars)}", labelStyle);

            int brakeBars = Mathf.RoundToInt(desiredBreakInput * maxBars);
            GUILayout.Label($"B: {new string('|', brakeBars)}", labelStyle);

            int handbrakeBars = Mathf.RoundToInt(desiredHandbrakeInput * maxBars);
            GUILayout.Label($"H: {new string('|', handbrakeBars)}", labelStyle);
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal(GUILayout.Width(400));
        {
            GUILayout.Label("S: ", labelStyle, GUILayout.Width(30));
            GUIStyle leftStyle = new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleRight };
            int steerLeftBars = desiredSteerInput < 0 ? Mathf.RoundToInt(-desiredSteerInput * (maxBars * 0.5f)) : 0;
            GUILayout.Label(new string('|', steerLeftBars), leftStyle, GUILayout.Width(100));
            GUILayout.Label("|", labelStyle, GUILayout.Width(10));
            GUIStyle rightStyle = new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleLeft };
            int steerRightBars = desiredSteerInput > 0 ? Mathf.RoundToInt(desiredSteerInput * (maxBars * 0.5f)) : 0;
            GUILayout.Label(new string('|', steerRightBars), rightStyle, GUILayout.Width(100));
        }

        GUILayout.EndHorizontal();
    }
}
