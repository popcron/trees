using UnityEngine;

public class RaycastSpring : MonoBehaviour
{
    public Rigidbody carRigidbody;
    public Transform tyreVisual;
    public WheelSettings settings;
    public float steeringAngle = 0f;
    public float gasInput = 0f;
    public float brakeInput = 0f;
    public float handbrakeInput = 0f;
    public float suspensionRestDistance = 5f;               
    public float wheelRadius = 0.4f;                        
    public float wheelAngularVelocity;                      
    public float wheelRollAngle;
    public float normalizedLongitudinalSlip;
    public float normalizedLateralSlip;
    public float normalizedWheelSpeed;
    public bool grounded;
    public float currentCompression;
    public Vector3 currentSpringDirection;
    public Vector3 currentSpringOrigin;

    private float tyreVisualYaw;
    private bool tyreVisualYawCached;

    private float WheelInertia => 0.5f * settings.tyreMass * wheelRadius * wheelRadius;

    private void Awake()
    {
        if (tyreVisual != null)
        {
            tyreVisualYaw = tyreVisual.localEulerAngles.y;
            tyreVisualYawCached = true;
        }
    }

    private void FixedUpdate()
    {
        Process(Time.fixedDeltaTime);
    }

    private void LateUpdate()
    {
        UpdateTyreVisual();
    }

    public void UpdateTyreVisual()
    {
        Ray ray = GetRay();
        float distance = TryGetGround(out RaycastHit ground, out _) ? ground.distance : suspensionRestDistance;
        tyreVisual.position = ray.origin + ray.direction * (distance - wheelRadius);
        wheelRollAngle = Mathf.Repeat(wheelRollAngle + wheelAngularVelocity * Mathf.Rad2Deg * Time.deltaTime, 360f);
        if (!tyreVisualYawCached)
        {
            tyreVisualYaw = tyreVisual.localEulerAngles.y;
            tyreVisualYawCached = true;
        }

        tyreVisual.localRotation = Quaternion.AngleAxis((wheelRollAngle + 90f) % 360f, Vector3.right) * Quaternion.AngleAxis(tyreVisualYaw, Vector3.up);
    }

    public Ray GetRay()
    {
        Vector3 rayOrigin = transform.position;
        Ray ray = new(rayOrigin, -transform.up);
        return ray;
    }

    public void Process(float delta)
    {
        float inertia = WheelInertia;
        if (gasInput > 0f)
        {
            normalizedWheelSpeed = Mathf.Clamp01(Mathf.Abs(wheelAngularVelocity * wheelRadius) / settings.forwardTopSpeed);
            float engineTorque = settings.powerCurve.Evaluate(normalizedWheelSpeed) * gasInput * settings.engineTorqueScale;
            wheelAngularVelocity += engineTorque / inertia * delta;
        }

        if (brakeInput > 0f)
        {
            // show the wheel first
            if (wheelAngularVelocity > 1f)
            {
                float maxBrake = wheelAngularVelocity * inertia / delta;
                float appliedBrake = Mathf.Min(settings.brakeTorque * brakeInput, maxBrake);
                wheelAngularVelocity -= appliedBrake / inertia * delta;
            }
            else
            {
                // then go backwards
                float normalizedReverseWheelSpeed = Mathf.Clamp01(Mathf.Abs(wheelAngularVelocity * wheelRadius) / settings.reverseTopSpeed);
                float engineTorque = settings.powerCurve.Evaluate(normalizedReverseWheelSpeed) * brakeInput * settings.reverseTorqueScale;
                wheelAngularVelocity -= engineTorque / inertia * delta;
            }
        }

        if (handbrakeInput > 0f && Mathf.Abs(wheelAngularVelocity) > 0.01f)
        {
            float maxBrake = Mathf.Abs(wheelAngularVelocity) * inertia / delta;
            float appliedBrake = maxBrake * handbrakeInput;
            wheelAngularVelocity -= Mathf.Sign(wheelAngularVelocity) * appliedBrake / inertia * delta;
        }

        grounded = TryGetGround(out RaycastHit ground, out Ray ray);
        if (!grounded)
        {
            currentCompression = 0f;
        }

        if (grounded)
        {
            float carShare = carRigidbody.mass / 4f; // assuming 4 wheels

            // suspension
            Vector3 springDirection = ground.normal;
            Vector3 mountVelocity = carRigidbody.GetPointVelocity(ray.origin);
            Vector3 contactVelocity = carRigidbody.GetPointVelocity(ground.point);
            float offset = suspensionRestDistance - ground.distance;
            currentCompression = offset;
            currentSpringDirection = springDirection;
            currentSpringOrigin = ray.origin;
            float springVelocity = Vector3.Dot(springDirection, mountVelocity);
            float damper = 2f * settings.dampingRatio * Mathf.Sqrt(settings.springStrength * carShare);
            float force = Mathf.Max(0f, (offset * settings.springStrength) - (springVelocity * damper));
            carRigidbody.AddForceAtPosition(springDirection * force, ray.origin);

            // lateral friction (raw)
            Vector3 steeringDirection = transform.right;
            float lateralVelocity = Vector3.Dot(steeringDirection, contactVelocity);
            normalizedLateralSlip = Mathf.Clamp01(Mathf.Abs(lateralVelocity) / settings.forwardTopSpeed);
            float lateralGrip = settings.lateralGripCurve.Evaluate(normalizedLateralSlip);
            float lateralForce = -lateralVelocity * lateralGrip * settings.lateralStiffness;

            // longitudinal friction (raw)
            Vector3 accelerationDirection = transform.forward;
            float forwardVelocity = Vector3.Dot(accelerationDirection, contactVelocity);
            float wheelSurfaceVelocity = wheelAngularVelocity * wheelRadius;
            float slipVelocity = forwardVelocity - wheelSurfaceVelocity;
            normalizedLongitudinalSlip = Mathf.Clamp01(Mathf.Abs(slipVelocity) / settings.forwardTopSpeed);
            float longitudinalGrip = settings.longitudinalGripCurve.Evaluate(normalizedLongitudinalSlip);
            float longitudinalForce = -slipVelocity * longitudinalGrip * settings.longitudinalStiffness;

            // friction circle
            float normalLoad = Mathf.Max(force, 0f);
            float frictionBudget = normalLoad * settings.peakFrictionCoefficient;
            float frictionMagnitude = Mathf.Sqrt(lateralForce * lateralForce + longitudinalForce * longitudinalForce);
            if (frictionMagnitude > frictionBudget && frictionBudget > 0f)
            {
                float scale = frictionBudget / frictionMagnitude;
                lateralForce *= scale;
                longitudinalForce *= scale;
            }

            carRigidbody.AddForceAtPosition(steeringDirection * lateralForce, ground.point);
            carRigidbody.AddForceAtPosition(accelerationDirection * longitudinalForce, ground.point);

            // reaction torque (uses post-budget longitudinal force)
            wheelAngularVelocity += -longitudinalForce * wheelRadius / inertia * delta;

            // viscous rolling resistance: torque proportional to wheel rotation speed
            float resistanceTorque = -wheelAngularVelocity * settings.rollingResistance;
            float maxStopTorque = Mathf.Abs(wheelAngularVelocity) * inertia / delta;
            float appliedTorque = Mathf.Sign(resistanceTorque) * Mathf.Min(Mathf.Abs(resistanceTorque), maxStopTorque);
            wheelAngularVelocity += appliedTorque / inertia * delta;
        }

        float forwardRedline = settings.forwardTopSpeed * 1.2f / wheelRadius;
        float reverseRedline = settings.reverseTopSpeed * 1.2f / wheelRadius;
        wheelAngularVelocity = Mathf.Clamp(wheelAngularVelocity, -reverseRedline, forwardRedline);
        Vector3 localEuler = transform.localEulerAngles;
        localEuler.y = steeringAngle;
        transform.localEulerAngles = localEuler;
    }

    private void OnDrawGizmosSelected()
    {
        Ray ray = GetRay();
        Vector3 springDirection = -ray.direction;
        Vector3 restPoint = ray.origin + ray.direction * suspensionRestDistance;

        EditorGizmos.color = Color.gray;
        EditorGizmos.DrawRay(ray, suspensionRestDistance, 2f);
        EditorGizmos.DrawWireDisc(restPoint, springDirection, 0.1f);

        if (TryGetGround(out RaycastHit ground, out _))
        {
            float compression = suspensionRestDistance - ground.distance;
            bool compressed = compression >= 0f;

            EditorGizmos.color = compressed ? Color.green : Color.yellow;
            EditorGizmos.DrawLine(ray.origin, ground.point, 3f);
            EditorGizmos.SphereHandleCap(0, ground.point, Quaternion.identity, 0.08f);
            EditorGizmos.DrawRay(ground.point, ground.normal * 0.5f, 2f);

            EditorGizmos.color = compressed ? Color.cyan : Color.red;
            EditorGizmos.DrawLine(restPoint, ground.point, 3f);

            Vector3 tyreVelocity = carRigidbody.GetPointVelocity(ray.origin);
            Vector3 normalDirection = ground.normal;
            float velocity = Vector3.Dot(normalDirection, tyreVelocity);
            float carShare = carRigidbody.mass / 4f;
            float damper = 2f * settings.dampingRatio * Mathf.Sqrt(settings.springStrength * carShare);
            float force = (compression * settings.springStrength) - (velocity * damper);
            float arrowSize = Mathf.Clamp(force / settings.springStrength, -1.5f, 1.5f);

            EditorGizmos.color = Color.magenta;
            EditorGizmos.ArrowHandleCap(0, ray.origin, Quaternion.LookRotation(normalDirection * Mathf.Sign(arrowSize)), Mathf.Abs(arrowSize));
        }
        else
        {
            EditorGizmos.color = Color.red;
            EditorGizmos.SphereHandleCap(0, restPoint, Quaternion.identity, 0.08f);
        }

        // wheel
        EditorGizmos.color = Color.blue;
        EditorGizmos.DrawWireDisc(ray.origin, transform.right, wheelRadius);
    }

    public bool TryGetGround(out RaycastHit hit, out Ray ray)
    {
        ray = GetRay();
        return Raycasting.TryGetClosestHit(ray, suspensionRestDistance, OnlyNonChildren, out hit);
    }

    private bool OnlyNonChildren(RaycastHit hit)
    {
        return !hit.collider.transform.IsChildOf(carRigidbody.transform) && hit.normal.y > 0.3f;
    }
}