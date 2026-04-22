using System.Collections.Generic;
using UnityEngine;

public class TriggerColliderCollector : MonoBehaviour
{
    public List<Collider> colliders = new();
    public List<Collider> previousColliders = new();

    private void FixedUpdate()
    {
        previousColliders.Clear();
        previousColliders.AddRange(colliders);
        colliders.Clear();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!colliders.Contains(other))
        {
            colliders.Add(other);
        }
    }
}