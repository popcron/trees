using System.Collections.Generic;
using UnityEngine;

public static class HoldAnchors
{
    public const string Tag = "Hold Anchor";

    public static void Collect(Rigidbody rigidbody, List<Transform> into)
    {
        into.Clear();
        Transform root = rigidbody.transform;
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

    public static int Count(Rigidbody rigidbody)
    {
        int count = 0;
        Transform root = rigidbody.transform;
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
}
