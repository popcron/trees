using UnityEngine;

public class IncreasedCollisionForce : MonoBehaviour
{
    public float pushPower = 10f;

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody otherRigidbody = collision.rigidbody;
        if (otherRigidbody != null)
        {
            Vector3 pushForce = collision.relativeVelocity.normalized * pushPower;
            otherRigidbody.AddForce(pushForce, ForceMode.Impulse);
        }
    }
}