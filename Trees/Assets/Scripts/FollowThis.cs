using UnityEngine;

public class FollowThis : MonoBehaviour
{
    public RigidbodyAgent[] agents = { };

    private void Reset()
    {
        agents = FindObjectsByType<RigidbodyAgent>();
    }

    private void OnDrawGizmos()
    {
        Vector3 destination = GetDestination();
        EditorGizmos.color = Color.red;
        EditorGizmos.ConeHandleCap(0, destination, Quaternion.LookRotation(Vector3.down), 0.2f);
        EditorGizmos.DrawWireDisc(destination, Vector3.up, 0.2f);
    }

    private void Update()
    {
        Vector3 destination = GetDestination();
        for (int i = 0; i < agents.Length; i++)
        {
            RigidbodyAgent agent = agents[i];
            if (agent.gameObject.activeInHierarchy && agent.agent.isOnNavMesh)
            {
                agent.agent.SetDestination(destination);
            }
        }
    }

    public Vector3 GetDestination()
    {
        Ray rayDown = new(transform.position + Vector3.up * 0.1f, Vector3.down);
        if (Raycasting.TryGetClosestHit(rayDown, 100f, IgnoreRigidbodies, out RaycastHit hit))
        {
            return hit.point + hit.normal * 0.1f;
        }

        return transform.position;
    }

    private static bool IgnoreRigidbodies(RaycastHit hit)
    {
        return hit.collider.attachedRigidbody == null;
    }
}
