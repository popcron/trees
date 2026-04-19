using UnityEngine;

public class FlyingController : MonoBehaviour
{
    public Rigidbody rigidbody;
    public float maxSpeed = 5f;
    public float maxAcceleration = 12f;
    public float lookSensitivity = 1f;
    public float maxLookSpeed = 40f;
    public float slowSpeedMultiplier = 0.4f;
    public float slowAccelerationMultiplier = 0.4f;
    public float slowLookSpeedMultiplier = 0.4f;
    public bool lockCursor;

    private float pitch;
    private float yaw;

    private void OnEnable()
    {
        Vector3 euler = rigidbody.rotation.eulerAngles;
        pitch = euler.x > 180f ? euler.x - 360f : euler.x;
        yaw = euler.y;
    }

    private void OnDisable()
    {
        rigidbody.angularVelocity = Vector3.zero;
        rigidbody.linearVelocity = Vector3.zero;
    }

    private void Update()
    {
        if (lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        PlayerInputState input = PlayerInputState.Get();
        float delta = Time.deltaTime;
        float speed = maxSpeed;
        float acceleration = maxAcceleration;
        float lookSpeed = maxLookSpeed;
        if (input.IsButtonPressed(PlayerInputState.Button.Shift))
        {
            speed *= slowSpeedMultiplier;
            acceleration *= slowAccelerationMultiplier;
            lookSpeed *= slowLookSpeedMultiplier;
        }

        pitch -= input.look.y * lookSensitivity;
        yaw += input.look.x * lookSensitivity;
        pitch = Mathf.Clamp(pitch, -89f, 89f);
        Quaternion desiredRotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredVelocity = rigidbody.rotation * input.Movement3D * speed;
        Vector3 velocityDelta = desiredVelocity - rigidbody.linearVelocity;
        rigidbody.linearVelocity += velocityDelta * acceleration * delta;
        Quaternion rotationDelta = desiredRotation * Quaternion.Inverse(rigidbody.rotation);
        rotationDelta.ToAngleAxis(out float angle, out Vector3 axis);
        if (angle > 180f)
        {
            angle -= 360f;
        }

        Vector3 desiredAngularVelocity = axis * angle * Mathf.Deg2Rad * lookSpeed;
        rigidbody.angularVelocity = desiredAngularVelocity;
    }
}
