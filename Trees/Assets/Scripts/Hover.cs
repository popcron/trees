using UnityEngine;
using UnityEngine.Events;

public class Hover : MonoBehaviour
{
    private static readonly RaycastHit[] results = new RaycastHit[32];

    public Rigidbody rigidbody;
    public float baseOffset = 0f;
    public float height = 0.5f;
    public float stiffness = 50f;
    public float damping = 5f;
    public float maxDistance = 10f;
    public float drag = 0f;
    public int frameInterval = 1;
    public AnimationCurve heightOverTime = AnimationCurve.Linear(0f, 0f, 1f, 0f);
    public float heightIdleAnimation = 0.1f;
    public float heightWhenMoving = 1f;
    public bool hovering;
    public UnityEvent onStartHovering;

    private int frame;
    private float animationTime;
    private bool hasJumped;

    private void Awake()
    {
        frame = GetEntityId().GetHashCode() & frameInterval;
        animationTime = GetEntityId().GetHashCode() / (float)int.MaxValue;
    }

    public Ray GetRay()
    {
        return new Ray(rigidbody.position + Vector3.up * baseOffset, Vector3.down);
    }

    private void OnDrawGizmosSelected()
    {
        Ray ray = GetRay();
        EditorGizmos.color = Color.cyan;
        if (TryGetClosestHit(out RaycastHit hit))
        {
            EditorGizmos.DrawLine(ray.origin, hit.point);
            EditorGizmos.DrawWireDisc(hit.point, hit.normal, 0.1f);
            EditorGizmos.ArrowHandleCap(0, hit.point, Quaternion.LookRotation(hit.normal), 0.2f);
        }
        else
        {
            EditorGizmos.DrawRay(ray.origin, ray.direction * maxDistance);
            EditorGizmos.ArrowHandleCap(0, ray.origin + ray.direction * maxDistance, Quaternion.LookRotation(ray.direction), 0.2f);
        }
    }

    private void Reset()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        float delta = Time.fixedDeltaTime;
        if (hasJumped)
        {
            if (rigidbody.linearVelocity.y > 0f)
            {
                return;
            }
        }

        frame++;
        if (frame >= frameInterval)
        {
            frame = 0;
            if (hovering != HoverAboveGround(delta * frameInterval))
            {
                hovering = !hovering;
                if (hovering)
                {
                    hasJumped = false;
                    onStartHovering.Invoke();
                }
            }
        }
    }

    public bool Jump(float height)
    {
        if (!hasJumped && hovering)
        {
            hasJumped = true;
            Vector3 velocity = rigidbody.linearVelocity;
            velocity.y = Mathf.Sqrt(2f * height * Physics.gravity.magnitude);
            rigidbody.linearVelocity = velocity;
            return true;
        }

        return false;
    }

    private bool HoverAboveGround(float delta)
    {
        Vector3 velocity = rigidbody.linearVelocity;
        if (TryGetClosestHit(out RaycastHit hit))
        {
            animationTime += velocity.magnitude * heightWhenMoving * delta;
            animationTime += heightIdleAnimation * delta;
            if (animationTime > 100f)
            {
                animationTime = -100f;
            }

            float animatedHeight = height + heightOverTime.Evaluate(animationTime % 1f);
            float distance = hit.distance;
            float error = animatedHeight - distance;
            float verticalSpeed = rigidbody.linearVelocity.y;
            float force = error * stiffness - verticalSpeed * damping;
            rigidbody.AddForce(Vector3.up * force, ForceMode.Acceleration);
            if (drag > 0f)
            {
                Vector3 horizontalVelocity = rigidbody.linearVelocity;
                horizontalVelocity.y = 0f;
                Vector3 dragForce = -horizontalVelocity * drag;
                rigidbody.AddForce(dragForce, ForceMode.Acceleration);

                Vector3 angularVelocity = rigidbody.angularVelocity;
                Vector3 angularDragForce = -angularVelocity * drag;
                rigidbody.AddTorque(angularDragForce, ForceMode.Acceleration);
            }

            return true;
        }

        return false;
    }

    private bool TryGetClosestHit(out RaycastHit hit)
    {
        hit = default;
        Ray ray = GetRay();
        int count = Physics.RaycastNonAlloc(ray, results, maxDistance);
        float closestDistance = float.MaxValue;
        int closestIndex = -1;
        for (int i = 0; i < count; i++)
        {
            hit = results[i];
            if (hit.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            float distance = hit.distance;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        if (closestIndex != -1)
        {
            hit = results[closestIndex];
            return true;
        }
        else
        {
            return false;
        }
    }
}
