using GOAP;
using System.Collections.Generic;
using UnityEngine;

public class DoSitHandler : ActionHandler<DoSit>
{
    private static readonly List<Transform> anchorBuffer = new();

    public override bool TryComplete(Actor actor, Layer layer, Goal activeGoal, Action action, float delta)
    {
        Unit unit = actor.GetComponent<Unit>();
        if (unit.IsSeated)
        {
            return true;
        }

        if (TryFindSeatAnchor(unit, out Transform anchor, out Rigidbody support))
        {
            SitCandidate candidate = new()
            {
                position = anchor.position,
                rotation = anchor.rotation,
                supportNormal = anchor.up,
                support = support,
                supportCollider = null,
                anchor = anchor,
            };
            unit.BindSeat(candidate);
            return true;
        }

        LayerMask supportMask = ~LayerMask.GetMask("Unit", "Ignore Raycast");
        LayerMask clearanceMask = ~LayerMask.GetMask("Unit", "Ignore Raycast");
        Vector3 forward = Quaternion.Euler(0f, unit.bodyYaw, 0f) * Vector3.forward;
        if (!SitProbe.TryFind(unit.transform.position, Vector3.up, forward, supportMask, clearanceMask, unit.transform, out SitCandidate probed))
        {
            return false;
        }

        unit.BindSeat(probed);
        return true;
    }

    private static bool TryFindSeatAnchor(Unit unit, out Transform bestAnchor, out Rigidbody bestSupport)
    {
        bestAnchor = null;
        bestSupport = null;
        float bestDistance = float.MaxValue;
        List<Collider> colliders = unit.interactionTrigger.previousColliders;
        for (int i = 0; i < colliders.Count; i++)
        {
            Collider collider = colliders[i];
            Rigidbody rb = collider.attachedRigidbody;
            Transform root = rb != null ? rb.transform : collider.transform;
            SeatAnchors.Collect(root, anchorBuffer);
            for (int j = 0; j < anchorBuffer.Count; j++)
            {
                Transform anchor = anchorBuffer[j];
                if (Unit.IsSeatTaken(anchor))
                {
                    continue;
                }

                float distance = (anchor.position - unit.transform.position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestAnchor = anchor;
                    bestSupport = rb;
                }
            }
        }

        return bestAnchor != null;
    }
}
