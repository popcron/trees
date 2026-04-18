using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class Pathfinding
{
    public const float NearZeroSquared = 0.02f;
    public static readonly Dictionary<EntityId, Agent> agents = new();

    public static Agent Register(EntityId entityId, Settings settings)
    {
        Agent agent = new(settings);
        agents.Add(entityId, agent);
        return agent;
    }

    public static void Unregister(EntityId entityId)
    {
        agents.Remove(entityId);
    }

    [Serializable]
    public class Settings
    {
        public float radius = 0.333f;
        public float jumpHeight = 5f;
        public float dropThreshold = 0.5f;
        public float separationWeight = 1f;
        public float separationRadiusMultiplier = 3f;
        public float ledgeDistanceAdd = 0.1f;
        public float slowDownRadiusMultiplier = 3f;
        public float velocityReverseThreshold = -0.1f;
        public float baseOffset = -0.5f;
        public float arriveHysteresisMultiplier = 1.5f;
        public float pathRecomputeInterval = 0.25f;
        public float separationFalloffInsideSlowDown = 1f;
        public float lookAheadDistanceMultiplier = 4f;
        public float lookAheadMaxSlopeRatio = 1f;
    }

    [Serializable]
    public class Agent
    {
        public readonly NavMeshPath navPath;
        public readonly Settings settings;
        public Vector3 position;
        public Vector3 previousVelocity;
        public Vector2 lastMoveInput;
        public bool arrived;
        public Vector3 cachedDestination;
        public Vector3 effectiveDestination;
        public float pathTimer;

        public float radius => settings.radius;
        public float separationWeight => settings.separationWeight;
        public float separationRadius => radius * settings.separationRadiusMultiplier;
        public float slowDownRadius => radius * settings.slowDownRadiusMultiplier;
        public float lookAheadDistance => radius * settings.lookAheadDistanceMultiplier;

        public Agent(Settings settings)
        {
            this.settings = settings;
            navPath = new NavMeshPath();
        }

        public bool TryResolve(Rigidbody rigidbody, Vector3 destination, float delta, out Vector2 moveInput)
        {
            moveInput = default;
            position = rigidbody.position + new Vector3(0f, -radius + settings.baseOffset, 0f);
            Vector3 currentVelocity = rigidbody.linearVelocity;
            currentVelocity.y = 0f;
            previousVelocity = currentVelocity;

            if ((destination - cachedDestination).sqrMagnitude > NearZeroSquared)
            {
                arrived = false;
                cachedDestination = destination;
                effectiveDestination = destination;
                pathTimer = 0f;
                lastMoveInput = Vector2.zero;
            }

            float arriveSqr = radius * radius;
            float hysteresis = settings.arriveHysteresisMultiplier;
            float leaveSqr = arriveSqr * hysteresis * hysteresis;
            if (arrived)
            {
                Vector3 stickyDelta = effectiveDestination - position;
                stickyDelta.y = 0f;
                if (stickyDelta.sqrMagnitude < leaveSqr)
                {
                    return false;
                }

                arrived = false;
            }

            Vector3[] corners = navPath.corners;
            pathTimer -= delta;
            bool needRecompute = pathTimer <= 0f || corners.Length < 2;
            if (needRecompute)
            {
                if (!NavMesh.SamplePosition(position, out NavMeshHit hit, radius * 2f, ~0))
                {
                    if (lastMoveInput.sqrMagnitude < NearZeroSquared)
                    {
                        return false;
                    }

                    moveInput = lastMoveInput;
                    return true;
                }

                NavMesh.CalculatePath(hit.position, destination, ~0, navPath);
                pathTimer = settings.pathRecomputeInterval;
                corners = navPath.corners;
                if (corners.Length >= 1)
                {
                    effectiveDestination = corners[corners.Length - 1];
                }
            }

            if (corners.Length < 2)
            {
                if (lastMoveInput.sqrMagnitude < NearZeroSquared)
                {
                    return false;
                }

                moveInput = lastMoveInput;
                return true;
            }

            Vector3 flatDelta = effectiveDestination - position;
            flatDelta.y = 0f;
            float flatSqrDist = flatDelta.sqrMagnitude;
            if (flatSqrDist < leaveSqr)
            {
                arrived = true;
                lastMoveInput = Vector2.zero;
                return false;
            }

            for (int i = 0; i < corners.Length - 1; i++)
            {
                Debug.DrawLine(corners[i], corners[i + 1], Color.green);
            }

            Vector3 lookAhead = ComputeLookAhead(corners, position, lookAheadDistance);
            Debug.DrawLine(position, lookAhead, Color.cyan);

            Vector3 toLookAhead = lookAhead - position;
            toLookAhead.y = 0f;
            if (toLookAhead.sqrMagnitude > NearZeroSquared)
            {
                Vector3 horizontal = toLookAhead.normalized;
                moveInput = new Vector2(horizontal.x, horizontal.z);
            }

            float distToDest = Mathf.Sqrt(flatSqrDist);
            float speedScale = Mathf.Clamp01(distToDest / slowDownRadius);
            float separationScale = Mathf.Lerp(1f, speedScale, settings.separationFalloffInsideSlowDown);
            Vector2 separation = ComputeSeparation(position);
            moveInput = Vector2.ClampMagnitude(moveInput + separation * separationWeight * separationScale, 1f);
            moveInput = Vector2.ClampMagnitude(moveInput, speedScale);
            lastMoveInput = moveInput;
            return true;
        }

        private static Vector3 ComputeLookAhead(Vector3[] corners, Vector3 from, float lookAheadDistance)
        {
            Vector2 fromXZ = new Vector2(from.x, from.z);
            float closestSqr = float.MaxValue;
            int closestSegment = 0;
            float closestT = 0f;

            for (int i = 0; i < corners.Length - 1; i++)
            {
                Vector2 aXZ = new Vector2(corners[i].x, corners[i].z);
                Vector2 bXZ = new Vector2(corners[i + 1].x, corners[i + 1].z);
                Vector2 segment2D = bXZ - aXZ;
                float segmentSqr = segment2D.sqrMagnitude;
                if (segmentSqr < NearZeroSquared)
                {
                    continue;
                }

                float t = Mathf.Clamp01(Vector2.Dot(fromXZ - aXZ, segment2D) / segmentSqr);
                Vector2 projected = aXZ + segment2D * t;
                float sqr = (fromXZ - projected).sqrMagnitude;
                if (sqr < closestSqr)
                {
                    closestSqr = sqr;
                    closestSegment = i;
                    closestT = t;
                }
            }

            float remaining = lookAheadDistance;
            int segmentIndex = closestSegment;
            float segmentParam = closestT;
            while (segmentIndex < corners.Length - 1)
            {
                Vector3 a = corners[segmentIndex];
                Vector3 b = corners[segmentIndex + 1];
                Vector3 ab = b - a;
                float segmentLength = new Vector2(ab.x, ab.z).magnitude;
                float remainingOnSegment = segmentLength * (1f - segmentParam);
                if (remainingOnSegment > remaining)
                {
                    float consumed = remaining / segmentLength;
                    return a + ab * (segmentParam + consumed);
                }

                remaining -= remainingOnSegment;
                segmentIndex++;
                segmentParam = 0f;
            }

            return corners[corners.Length - 1];
        }

        private Vector2 ComputeSeparation(Vector3 position)
        {
            float separationRadius = settings.radius * 3f;
            Vector2 force = Vector2.zero;
            foreach (Agent other in agents.Values)
            {
                if (other == this)
                {
                    continue;
                }

                Vector3 diff = position - other.position;
                diff.y = 0f;
                float sqrDist = diff.sqrMagnitude;
                float separationDist = Mathf.Max(separationRadius, other.separationRadius);
                float sqrSeparationDist = separationDist * separationDist;
                if (sqrDist >= sqrSeparationDist || sqrDist < NearZeroSquared)
                {
                    continue;
                }

                float dist = Mathf.Sqrt(sqrDist);
                float strength = 1f - dist / separationDist;
                force += new Vector2(diff.x, diff.z) / dist * strength;
            }

            return force;
        }
    }
}