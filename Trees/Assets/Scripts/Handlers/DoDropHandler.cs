using GOAP;
using UnityEngine;

public class DoDropHandler : ActionHandler<DoDrop>
{
    private static Rigidbody[] occupiedByBuffer = new Rigidbody[0];

    public override bool TryComplete(Actor actor, Layer layer, Goal activeGoal, Action action, float delta)
    {
        Unit unit = actor.GetComponent<Unit>();
        if (unit.carrying.Count == 0)
        {
            return false;
        }

        Pawn pawn = actor.GetComponentInChildren<Pawn>();
        int handCount = pawn.body.hands.Count;
        if (handCount == 0)
        {
            return false;
        }

        if (occupiedByBuffer.Length < handCount)
        {
            occupiedByBuffer = new Rigidbody[handCount];
        }

        unit.AssignHands(handCount, occupiedByBuffer);

        Rigidbody target = null;
        for (int step = 0; step < handCount; step++)
        {
            int index = (unit.preferredHandIndex + step) % handCount;
            if (occupiedByBuffer[index] != null)
            {
                target = occupiedByBuffer[index];
                break;
            }
        }

        if (target == null)
        {
            return false;
        }

        unit.carrying.Remove(target);
        target.transform.SetParent(null, worldPositionStays: true);
        target.isKinematic = false;
        return true;
    }
}
