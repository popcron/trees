using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class RigidbodyAgent : MonoBehaviour
{
    public static readonly Vector3[] cornersBuffer = new Vector3[256];

    public float velocityBreakThreshold = 1.05f;
    public float warpThreshold = 1.05f;
    public Rigidbody rigidbody;
    public NavMeshAgent agent;

    private float lastCollisionTime;
    private float contactTime;

    private void Reset()
    {
        rigidbody = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
    }

    private void OnDrawGizmos()
    {
        Color originalColor = Gizmos.color;

        if (agent.isOnNavMesh)
        {
            if (agent.remainingDistance > agent.radius * 0.5f)
            {
                // show path
                Gizmos.color = Color.yellow;
                int cornerCount = agent.path.GetCornersNonAlloc(cornersBuffer);
                for (int i = 0; i < cornerCount - 1; i++)
                {
                    Gizmos.DrawLine(cornersBuffer[i], cornersBuffer[i + 1]);
                }
            }
        }

        Gizmos.color = originalColor;
    }

    private void FixedUpdate()
    {
        Drive(Time.fixedDeltaTime);
    }

    public void Drive(float delta)
    {
        bool contact = Time.fixedTime - lastCollisionTime < 0.25f;
        if (contact)
        {
            contactTime += delta;
        }
        else
        {
            contactTime = 0f;
        }

        agent.updatePosition = false;
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        if (agent.isOnNavMesh)
        {
            agent.nextPosition = rigidbody.position;
            Vector3 linearVelocity = rigidbody.linearVelocity;
            if (agent.remainingDistance > agent.radius * 0.5f)
            {
                Vector3 horizontalVelocity = new(linearVelocity.x, 0f, linearVelocity.z);
                Vector3 horizontalDesired = new(agent.desiredVelocity.x, 0f, agent.desiredVelocity.z);
                float breakLimit = agent.speed * velocityBreakThreshold;
                bool flyingWithIntent = horizontalVelocity.sqrMagnitude > breakLimit * breakLimit && Vector3.Dot(horizontalVelocity, horizontalDesired) > 0f;
                if (!flyingWithIntent)
                {
                    linearVelocity.x = Mathf.MoveTowards(linearVelocity.x, agent.desiredVelocity.x, delta * agent.acceleration);
                    linearVelocity.z = Mathf.MoveTowards(linearVelocity.z, agent.desiredVelocity.z, delta * agent.acceleration);
                }
            }
            else
            {
                linearVelocity.x = Mathf.MoveTowards(linearVelocity.x, 0f, delta * agent.acceleration);
                linearVelocity.z = Mathf.MoveTowards(linearVelocity.z, 0f, delta * agent.acceleration);
            }

            if (contactTime > 0.5f)
            {
                float threshold = agent.radius * warpThreshold;
                Vector3 difference = rigidbody.position - agent.nextPosition;
                if (difference.sqrMagnitude > threshold * threshold)
                {
                    if (NavMesh.SamplePosition(rigidbody.position, out NavMeshHit hit, agent.radius, NavMesh.AllAreas))
                    {
                        agent.Warp(hit.position);
                    }
                    else
                    {
                        linearVelocity.x = Mathf.MoveTowards(linearVelocity.x, -difference.x, delta * agent.acceleration);
                        linearVelocity.z = Mathf.MoveTowards(linearVelocity.z, -difference.z, delta * agent.acceleration);
                    }
                }
            }

            rigidbody.linearVelocity = linearVelocity;
        }
        else
        {
            if (contactTime > 0.5f)
            {
                if (NavMesh.SamplePosition(rigidbody.position, out NavMeshHit hit, agent.radius * warpThreshold, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        lastCollisionTime = Time.fixedTime;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!agent.isOnNavMesh)
        {
            agent.Warp(rigidbody.position);
        }
    }
}
