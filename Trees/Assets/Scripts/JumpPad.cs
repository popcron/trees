using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float height = 8f;
    public Vector3 direction = Vector3.up;

    private void OnDrawGizmosSelected()
    {
        EditorGizmos.color = Color.yellow;
        Vector3 direction = transform.rotation * this.direction.normalized;
        EditorGizmos.DrawRay(transform.position, direction * height);
        float arrowDensity = 0.5f;
        int arrows = Mathf.CeilToInt(height / arrowDensity);
        for (int i = 0; i < arrows; i++)
        {
            Vector3 position = transform.position;
            position += direction * (i * arrowDensity);
            EditorGizmos.ArrowHandleCap(0, position, Quaternion.LookRotation(direction), 0.5f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody != null)
        {
            Vector3 direction = transform.rotation * this.direction.normalized;
            other.attachedRigidbody.AddForce(direction * Mathf.Sqrt(2f * height * Physics.gravity.magnitude), ForceMode.VelocityChange);
        }
    }
}
