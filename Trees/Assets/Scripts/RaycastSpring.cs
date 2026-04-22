using UnityEngine;

public class RaycastSpring : MonoBehaviour
{
    public Rigidbody carRigidbody;
    public Transform tyreVisual;
    public WheelSettings settings;
    public float steeringAngle = 0f;
    public float gasInput = 0f;
    public float brakeInput = 0f;
    public float suspensionRestDistance = 5f;
    public float wheelRadius = 0.4f;
    public float wheelAngularVelocity;
    public float normalizedLongitudinalSlip;
    public float normalizedLateralSlip;
    public float normalizedWheelSpeed;

    private float WheelInertia => 0.5f * settings.tyreMass * wheelRadius * wheelRadius;

    private void FixedUpdate()
    {
        Process(Time.fixedDeltaTime);
    }

    private void LateUpdate()
    {
        UpdateTireVisual();
    }

    public void UpdateTireVisual()
    {
        Ray ray = GetRay();
        float distance = TryGetGround(out RaycastHit ground, out _) ? ground.distance : suspensionRestDistance;
        tyreVisual.position = ray.origin + ray.direction * (distance - wheelRadius);
        tyreVisual.rotation = transform.rotation;
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
                float engineTorque = settings.reversePowerCurve.Evaluate(normalizedReverseWheelSpeed) * brakeInput * settings.reverseTorqueScale;
                wheelAngularVelocity -= engineTorque / inertia * delta;
            }
        }

        if (TryGetGround(out RaycastHit ground, out Ray ray))
        {
            float carShare = carRigidbody.mass / 4f; // assuming 4 wheels

            // suspension
            Vector3 springDirection = -ray.direction;
            Vector3 tyreWorldVelocity = carRigidbody.GetPointVelocity(ray.origin);
            float offset = suspensionRestDistance - ground.distance;
            float springVelocity = Vector3.Dot(springDirection, tyreWorldVelocity);
            float force = (offset * settings.springStrength) - (springVelocity * settings.springDamper);
            carRigidbody.AddForceAtPosition(springDirection * force, ray.origin);
        
            // lateral friction
            Vector3 steeringDirection = transform.right;
            float lateralVelocity = Vector3.Dot(steeringDirection, tyreWorldVelocity);
            normalizedLateralSlip = Mathf.Clamp01(Mathf.Abs(lateralVelocity) / settings.forwardTopSpeed);
            float lateralGrip = settings.lateralGripCurve.Evaluate(normalizedLateralSlip);
            float lateralForce = -lateralVelocity * lateralGrip * carShare / delta;
            carRigidbody.AddForceAtPosition(steeringDirection * lateralForce, ray.origin);

            // longitudinal friction
            float wheelEquivMass = inertia / (wheelRadius * wheelRadius);
            float reducedMass = (wheelEquivMass * carShare) / (wheelEquivMass + carShare);
            Vector3 accelerationDirection = transform.forward;
            float forwardVelocity = Vector3.Dot(accelerationDirection, tyreWorldVelocity);
            float wheelSurfaceVelocity = wheelAngularVelocity * wheelRadius;
            float slipVelocity = forwardVelocity - wheelSurfaceVelocity;
            normalizedLongitudinalSlip = Mathf.Clamp01(Mathf.Abs(slipVelocity) / settings.forwardTopSpeed);
            float longitudinalGrip = settings.longitudinalGripCurve.Evaluate(normalizedLongitudinalSlip);
            float longitudinalForce = -slipVelocity * longitudinalGrip * reducedMass / delta;
            carRigidbody.AddForceAtPosition(accelerationDirection * longitudinalForce, ray.origin);
        
            // reaction torque
            wheelAngularVelocity += -longitudinalForce * wheelRadius / inertia * delta;
        }

        transform.localRotation = Quaternion.Euler(0f, steeringAngle, 0f);
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
            float velocity = Vector3.Dot(springDirection, tyreVelocity);
            float force = (compression * settings.springStrength) - (velocity * settings.springDamper);
            float arrowSize = Mathf.Clamp(force / settings.springStrength, -1.5f, 1.5f);

            EditorGizmos.color = Color.magenta;
            EditorGizmos.ArrowHandleCap(0, ray.origin, Quaternion.LookRotation(springDirection * Mathf.Sign(arrowSize)), Mathf.Abs(arrowSize));
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
        return !hit.collider.transform.IsChildOf(carRigidbody.transform);
    }
}