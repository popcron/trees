using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class GravityAligner : MonoBehaviour
{
    public Vector3 gravityDirection = Vector3.down;
    public float stiffness = 8f;
    public float damping = 6f;
    public Rigidbody rigidbody;

    private void Reset()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        AlignToGravity(Time.fixedDeltaTime);
    }

    private void AlignToGravity(float delta)
    {
        Vector3 targetUp = -gravityDirection.normalized;
        Vector3 currentUp = transform.up;
        Quaternion error = Quaternion.FromToRotation(currentUp, targetUp);
        error.ToAngleAxis(out float degreeAngles, out Vector3 axis);
        if (axis.sqrMagnitude < 0.001f || Mathf.Abs(degreeAngles) < 0.01f)
        {
            return;
        }

        if (degreeAngles > 180f)
        {
            degreeAngles -= 360f;
        }

        Vector3 targetAngularVelocity = axis * (degreeAngles * Mathf.Deg2Rad * stiffness);
        float t = 1f - Mathf.Exp(-damping * delta);
        rigidbody.angularVelocity = Vector3.Lerp(rigidbody.angularVelocity, targetAngularVelocity, t);
    }
}