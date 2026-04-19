using UnityEngine;
using UnityEngine.Events;

public class Raycast : MonoBehaviour
{
    public float maxDistance = 100f;
    public Vector3 direction = Vector3.forward;
    public LayerMask layerMask = ~0;
    public UnityEvent<RaycastHit> onHit = new();

    private void Update()
    {
        if (Raycasting.TryGetClosestHit(GetRay(), maxDistance, layerMask, out RaycastHit closestHit))
        {
            onHit.Invoke(closestHit);
        }
    }

    public Ray GetRay()
    {
        return new(transform.position, transform.TransformDirection(direction));
    }
}