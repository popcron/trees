using UnityEngine;
using UnityEngine.Events;

public class TriggerEventCallbacks : MonoBehaviour
{
    public UnityEvent<Collider> onTriggerEnter = new();
    public UnityEvent<Collider> onTriggerExit = new();
    public UnityEvent<Collider> onTriggerStay = new();

    private void OnTriggerEnter(Collider other)
    {
        onTriggerEnter.Invoke(other);
    }

    private void OnTriggerExit(Collider other)
    {
        onTriggerExit.Invoke(other);
    }

    private void OnTriggerStay(Collider other)
    {
        onTriggerStay.Invoke(other);
    }
}
