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
    public TriggerColliderCollector interactionTrigger;
    public Color color = Color.white;
    public bool grounded;
    public float bodyYaw;
    public Vector2 headPitchYaw;
    public Vector2 eyePitchYaw;
    public float carryStrength = 2f;
    public float eyeDistance = 5f;
    public float maxSpeed = 3f;
    public float maxAcceleration = 40f;
    public float maxJumpHeight = 1.76f;
    public Transform groundChecker;
    public float groundDistance = 0.1f;
    public LayerMask groundLayerMask = ~0;
    public float groundCheckCooldown;
    public int preferredHandIndex = 0;
    public List<Rigidbody> carrying = new();
    public Rigidbody seatSupport;
    public Transform seatAnchor;
    public Vector3 seatLocalPosition;
    public Quaternion seatLocalRotation;
    public Vector3 seatLastSupportVelocity;
    public Vector3 seatLastSupportAngularVelocity;
    public float seatEjectAcceleration = 60f;
    public float seatEjectAngularAcceleration = 80f;
    public bool seatWasKinematic;

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
        if (!actor.state.Has<Seated>() && !actor.state.Has<Standing>())
        {
            actor.state.Add<Standing>();
        }
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
        actor.Act(delta);
        CheckIfGrounded(delta);
        ApplyItemDrag(delta);
        TrackSeat(delta);
    }

    public bool IsSeated
    {
        get { return actor.state.Has<Seated>(); }
    }

    public void BindSeat(SitCandidate candidate)
    {
        agent.enabled = false;
        seatWasKinematic = agent.rigidbody.isKinematic;
        agent.rigidbody.isKinematic = true;
        agent.rigidbody.linearVelocity = Vector3.zero;
        agent.rigidbody.angularVelocity = Vector3.zero;
        agent.rigidbody.position = candidate.position;
        agent.rigidbody.rotation = candidate.rotation;

        seatSupport = candidate.support;
        seatAnchor = candidate.anchor;
        if (seatSupport != null)
        {
            seatLocalPosition = seatSupport.transform.InverseTransformPoint(candidate.position);
            seatLocalRotation = Quaternion.Inverse(seatSupport.rotation) * candidate.rotation;
            seatLastSupportVelocity = seatSupport.linearVelocity;
            seatLastSupportAngularVelocity = seatSupport.angularVelocity;
        }
    }

    public void ReleaseSeat()
    {
        seatSupport = null;
        seatAnchor = null;
        agent.rigidbody.isKinematic = seatWasKinematic;
        agent.enabled = true;
    }

    private void TrackSeat(float delta)
    {
        if (!IsSeated)
        {
            return;
        }

        if (seatSupport == null)
        {
            return;
        }

        Vector3 linearDelta = seatSupport.linearVelocity - seatLastSupportVelocity;
        Vector3 angularDelta = seatSupport.angularVelocity - seatLastSupportAngularVelocity;
        float inverseDelta = 1f / delta;
        bool violent = linearDelta.magnitude * inverseDelta > seatEjectAcceleration
                    || angularDelta.magnitude * inverseDelta > seatEjectAngularAcceleration;

        seatLastSupportVelocity = seatSupport.linearVelocity;
        seatLastSupportAngularVelocity = seatSupport.angularVelocity;

        if (violent)
        {
            Eject();
            return;
        }

        Vector3 worldPosition = seatSupport.transform.TransformPoint(seatLocalPosition);
        Quaternion worldRotation = seatSupport.rotation * seatLocalRotation;
        agent.rigidbody.MovePosition(worldPosition);
        agent.rigidbody.MoveRotation(worldRotation);
    }

    private void Eject()
    {
        Vector3 inheritLinear = seatSupport != null ? seatSupport.GetPointVelocity(agent.rigidbody.position) : Vector3.zero;
        Vector3 inheritAngular = seatSupport != null ? seatSupport.angularVelocity : Vector3.zero;
        ReleaseSeat();
        agent.rigidbody.linearVelocity = inheritLinear;
        agent.rigidbody.angularVelocity = inheritAngular;
        actor.state.Remove(typeof(Seated).GetID());
        if (!actor.state.Has<Standing>())
        {
            actor.state.Add<Standing>();
        }
    }

    private void ApplyItemDrag(float delta)
    {
        if (IsSeated)
        {
            return;
        }

        float mass = GetHeldMass();
        if (mass == 0f)
        {
            return;
        }

        float factor = Mathf.Exp(-mass * delta);
        Vector3 velocity = agent.rigidbody.linearVelocity;
        velocity.x *= factor;
        velocity.z *= factor;
        agent.rigidbody.linearVelocity = velocity;
    }

    public float GetHeldMass()
    {
        float mass = 0f;
        for (int i = 0; i < carrying.Count; i++)
        {
            mass += carrying[i].mass;
        }

        return mass / carryStrength;
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

    public bool TryGetClosestInteractiveRigidbody(out Rigidbody closestRigidbody)
    {
        closestRigidbody = null;
        float threshold = float.MaxValue;
        for (int i = 0; i < interactionTrigger.previousColliders.Count; i++)
        {
            Collider collider = interactionTrigger.previousColliders[i];
            Rigidbody rigidbody = collider.attachedRigidbody;
            if (rigidbody != null)
            {
                float distanceSquared = (rigidbody.worldCenterOfMass - head.position).sqrMagnitude;
                if (distanceSquared < threshold)
                {
                    threshold = distanceSquared;
                    closestRigidbody = rigidbody;
                }
            }
        }

        return closestRigidbody != null;
    }

    public int CountUsedHands()
    {
        int used = 0;
        for (int i = 0; i < carrying.Count; i++)
        {
            used += HoldAnchors.Count(carrying[i]);
        }

        return used;
    }

    public int GetFreeHands(int handCount)
    {
        return handCount - CountUsedHands();
    }

    public void AssignHands(int handCount, Rigidbody[] occupiedBy)
    {
        for (int i = 0; i < handCount; i++)
        {
            occupiedBy[i] = null;
        }

        int cursor = preferredHandIndex % handCount;
        for (int i = 0; i < carrying.Count; i++)
        {
            Rigidbody rb = carrying[i];
            int needed = HoldAnchors.Count(rb);
            for (int k = 0; k < needed; k++)
            {
                while (occupiedBy[cursor] != null)
                {
                    cursor = (cursor + 1) % handCount;
                }

                occupiedBy[cursor] = rb;
                cursor = (cursor + 1) % handCount;
            }
        }
    }

    public static bool IsHeld(Rigidbody rigidbody)
    {
        for (int i = 0; i < all.Count; i++)
        {
            List<Rigidbody> list = all[i].carrying;
            for (int j = 0; j < list.Count; j++)
            {
                if (list[j] == rigidbody)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static bool IsSeatTaken(Transform anchor)
    {
        for (int i = 0; i < all.Count; i++)
        {
            if (all[i].seatAnchor == anchor)
            {
                return true;
            }
        }

        return false;
    }
}
