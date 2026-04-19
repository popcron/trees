using GOAP;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Actor))]
[RequireComponent(typeof(RigidbodyAgent))]
public class Unit : BaseBehaviour
{
    public static readonly List<Unit> all = new();

    public Actor actor;
    public RigidbodyAgent agent;
    public Transform head;
    public Color color = Color.white;
    public ControlFlags controlFlags;
    public Vector2 input;
    public bool grounded;
    public float bodyYaw;
    public Vector2 headPitchYaw;
    public Vector2 eyePitchYaw;
    public bool jump;
    public float eyeDistance = 5f;
    public float maxSpeed = 3f;
    public float maxAcceleration = 40f;
    public float maxJumpHeight = 1.76f;
    public Transform groundChecker;
    public float groundDistance = 0.1f;
    public LayerMask groundLayerMask = ~0;
    public float groundCheckCooldown;

    public Quaternion LookRotation
    {
        get
        {
            Quaternion bodyRotation = Quaternion.Euler(0f, bodyYaw, 0f);
            Quaternion headRotation = Quaternion.Euler(headPitchYaw.x, headPitchYaw.y, 0f);
            Quaternion eyeRotation = Quaternion.Euler(eyePitchYaw.x, eyePitchYaw.y, 0f);
            return bodyRotation * headRotation * eyeRotation;
        }
    }

    private void Reset()
    {
        actor = GetComponent<Actor>();
        agent = GetComponent<RigidbodyAgent>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        all.Add(this);
    }

    protected override void OnDisable()
    {
        all.Remove(this);
        base.OnDisable();
    }

    private void OnDrawGizmosSelected()
    {
        // ground forward
        Ray ray = GetGroundRay();
        EditorGizmos.color = Color.purple;
        if (TryGetGroundHit(out RaycastHit hit))
        {
            EditorGizmos.DrawLine(ray.origin, hit.point);
            EditorGizmos.ConeHandleCap(0, hit.point, Quaternion.LookRotation(hit.normal), 0.1f);
            EditorGizmos.DrawWireDisc(hit.point, transform.up, 0.2f);
        }
        else
        {
            EditorGizmos.DrawRay(ray, groundDistance);
            EditorGizmos.ConeHandleCap(0, ray, groundDistance, 0.1f);
            EditorGizmos.DrawWireDisc(ray.origin + ray.direction * groundDistance, transform.up, 0.2f);
        }
    }

    public Ray GetGroundRay()
    {
        return new Ray(groundChecker.position, groundChecker.forward);
    }

    public bool TryGetGroundHit(out RaycastHit hit)
    {
        Ray ray = GetGroundRay();
        return Raycasting.TryGetClosestHit(ray, groundDistance, groundLayerMask, out hit);
    }

    public void StareAt(Vector3 lookAt)
    {
        Vector3 direction = (lookAt - head.position).normalized;
        Vector3 localDirection = Quaternion.Inverse(Quaternion.Euler(0f, bodyYaw, 0f)) * direction;
        eyePitchYaw.x = -Mathf.Asin(localDirection.y) * Mathf.Rad2Deg;
        eyePitchYaw.y = Mathf.Atan2(localDirection.x, localDirection.z) * Mathf.Rad2Deg;
    }

    private void FixedUpdate()
    {
        float delta = Time.fixedDeltaTime;
        input = Vector2.zero;
        actor.Act(delta);
        CheckIfGrounded(delta);
    }

    private void CheckIfGrounded(float delta)
    {
        groundCheckCooldown -= delta;
        if (groundCheckCooldown < 0f)
        {
            groundCheckCooldown = 0f;
            grounded = TryGetGroundHit(out _);
        }
    }
}
