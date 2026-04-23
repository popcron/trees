using System.Collections.Generic;
using UnityEngine;

public static class SeatAnchors
{
    public const string Tag = "Seat Anchor";

    public static void Collect(Transform root, List<Transform> into)
    {
        into.Clear();
        int childCount = root.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child.CompareTag(Tag))
            {
                into.Add(child);
            }
        }
    }

    public static int Count(Transform root)
    {
        int count = 0;
        int childCount = root.childCount;
        for (int i = 0; i < childCount; i++)
        {
            if (root.GetChild(i).CompareTag(Tag))
            {
                count++;
            }
        }

        return count;
    }

    public static bool IsOccupied(Transform anchor)
    {
        for (int i = 0; i < Unit.all.Count; i++)
        {
            if (Unit.all[i].seatAnchor == anchor)
            {
                return true;
            }
        }

        return false;
    }
}
