using UnityEngine;
using UnityEngine.Events;

public class CollisionEventCallbacks : MonoBehaviour
{
    public UnityEvent<Collision> onCollisionEnter = new();
    public UnityEvent<Collision> onCollisionExit = new();
    public UnityEvent<Collision> onCollisionStay = new();

    private void OnCollisionEnter(Collision collision)
    {
        onCollisionEnter.Invoke(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        onCollisionExit.Invoke(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        onCollisionStay.Invoke(collision);
    }
}
