using UnityEngine;

public struct SitCandidate
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 supportNormal;
    public Rigidbody support;
    public Collider supportCollider;
    public Transform anchor;
}
