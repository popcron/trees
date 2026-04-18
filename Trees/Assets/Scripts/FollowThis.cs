using UnityEngine;
using UnityEngine.InputSystem;

public class FollowThis : MonoBehaviour
{
    public float speed = 3f;
    public float acceleration = 8f;
    public Pathfinding.Settings settings = new();

    private Pathfinding.Agent[] agents = { };
    private Rigidbody[] rigidbodies = { };

    private void OnEnable()
    {
        rigidbodies = FindObjectsByType<Rigidbody>();
        agents = new Pathfinding.Agent[rigidbodies.Length];
        for (int i = 0; i < agents.Length; i++)
        {
            Rigidbody rigidbody = rigidbodies[i];
            agents[i] = Pathfinding.Register(rigidbody.GetEntityId(), settings);
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            Pathfinding.Unregister(rigidbodies[i].GetEntityId());
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 destination = GetDestination();
        EditorGizmos.color = Color.red;
        EditorGizmos.ConeHandleCap(0, destination, Quaternion.LookRotation(Vector3.down), 0.2f);
        EditorGizmos.DrawWireDisc(destination, Vector3.up, 0.2f);

        // preview the radius
        EditorGizmos.color = Color.orange;
        Rigidbody[] rigidbodies = FindObjectsByType<Rigidbody>();
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            Vector3 position = rigidbodies[i].position;
            EditorGizmos.DrawWireDisc(position, Vector3.up, settings.radius);
        }
    }

    private void Update()
    {
        float delta = Time.deltaTime;
        UpdatePosition();
        Vector3 destination = GetDestination();
        for (int i = 0; i < agents.Length; i++)
        {
            Pathfinding.Agent agent = agents[i];
            Rigidbody rigidbody = rigidbodies[i];
            if (rigidbody.gameObject.activeSelf)
            {
                if (agent.TryResolve(rigidbody, destination, delta, out Vector2 moveInput))
                {
                    Vector3 velocity = rigidbody.linearVelocity;
                    velocity.x = Mathf.Lerp(velocity.x, moveInput.x * speed, acceleration * delta);
                    velocity.z = Mathf.Lerp(velocity.z, moveInput.y * speed, acceleration * delta);
                    rigidbody.linearVelocity = velocity;
                }
            }
        }
    }

    public void UpdatePosition()
    {
        Ray cameraRay = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Raycasting.TryGetClosestHit(cameraRay, 100f, IgnoreRigidbodies, out RaycastHit hit))
        {
            transform.position = hit.point + hit.normal * 0.1f;
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
