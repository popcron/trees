using UnityEngine;

public class CenterOfMass : MonoBehaviour
{
    public Rigidbody rigidbody;
    public Vector3 value;

    private void Reset()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        rigidbody.centerOfMass = value;
    }
}