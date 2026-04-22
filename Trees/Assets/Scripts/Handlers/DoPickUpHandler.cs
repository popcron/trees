using GOAP;
using System.Collections.Generic;
using UnityEngine;

public class DoPickUpHandler : ActionHandler<DoPickUp>
{
    private static readonly List<Transform> holdAnchorsBuffer = new();
    private static Rigidbody[] occupiedByBuffer = new Rigidbody[0];

    public override bool TryComplete(Actor actor, Layer layer, Goal activeGoal, Action action, float delta)
    {
        Unit unit = actor.GetComponent<Unit>();
        if (!unit.TryGetClosestInteractiveRigidbody(out Rigidbody rigidbody) || Unit.IsHeld(rigidbody))
        {
            return false;
        }

        Pawn pawn = actor.GetComponentInChildren<Pawn>();
        int handCount = pawn.body.hands.Count;

        HoldAnchors.Collect(rigidbody, holdAnchorsBuffer);
        int anchorCount = holdAnchorsBuffer.Count;
        if (anchorCount == 0 || anchorCount > unit.GetFreeHands(handCount))
        {
            return false;
        }

        unit.carrying.Add(rigidbody);
        rigidbody.isKinematic = true;
        rigidbody.transform.SetParent(pawn.body.transform, worldPositionStays: true);

        if (occupiedByBuffer.Length < handCount)
        {
            occupiedByBuffer = new Rigidbody[handCount];
        }

        unit.AssignHands(handCount, occupiedByBuffer);

        int anchorCursor = 0;
        bool snapped = false;
        for (int i = 0; i < handCount && anchorCursor < anchorCount; i++)
        {
            if (occupiedByBuffer[i] != rigidbody)
            {
                continue;
            }

            HandPart hand = pawn.body.hands[i];
            Transform anchor = holdAnchorsBuffer[anchorCursor++];
            if (!snapped && hand.carryAnchor != null)
            {
                Vector3 offset = hand.carryAnchor.position - anchor.position;
                rigidbody.transform.position += offset;
                snapped = true;
            }
        }

        return true;
    }
}
