using System;
using UnityEngine;

public class Decorator : MonoBehaviour
{
    public GameObject[] prefabs = { };
    public float density = 0.8f;
    public Vector3 center = Vector3.zero;
    public Vector2 size = new(10f, 10f);
    public int seed;

    private void Reset()
    {
        seed = (int)(Time.timeAsDouble % 1.0 * 12310293);
    }

    public Bounds GetBounds()
    {
        Vector3 center = transform.TransformPoint(this.center);
        Vector3 size = Vector3.Scale(new Vector3(this.size.x, 0f, this.size.y), transform.lossyScale);
        return new Bounds(center, size);
    }

    private void OnDrawGizmosSelected()
    {
        Bounds bounds = GetBounds();
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }

    [ContextMenu("Decorate")]
    public void Decorate()
    {
        // destroy existing children that are prefabs
        if (Application.isEditor && !Application.isPlaying)
        {
#if UNITY_EDITOR
            int childCount = transform.childCount;
            for (int i = childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (UnityEditor.PrefabUtility.IsPartOfAnyPrefab(child.gameObject))
                {
                    UnityEditor.Undo.DestroyObjectImmediate(child.gameObject);
                }
            }
#endif
        }

        System.Random rng = new(seed);
        Bounds bounds = GetBounds();
        float y = bounds.center.y;
        float area = Mathf.Max(1f, size.x) * Mathf.Max(1f, size.y);
        float radius = area / density;
        float radiusSquared = radius * radius;
        Vector2 min = new(bounds.min.x, bounds.min.z);
        Vector2 max = new(bounds.max.x, bounds.max.z);
        int maxAttempts = 200;
        int maxCount = Mathf.CeilToInt(area * density);
        int count = 0;
        Vector2Int[] positions = new Vector2Int[maxCount];
        for (int i = 0; i < maxCount; i++)
        {
            int attempt = 0;
            while (attempt < maxAttempts)
            {
                attempt++;
                Vector3 worldPosition = GetRandomPosition();
                Vector2Int position = Compress(worldPosition, radius);
                if (!TooClose(position))
                {
                    // check if theres a collider here
                    if (Physics.Linecast(worldPosition + Vector3.up * 0.5f, worldPosition + Vector3.up * 0.1f))
                    {
                        continue;
                    }

                    positions[count++] = position;
                    float yaw = (float)(rng.NextDouble() * 360.0);
                    Quaternion rotation = Quaternion.Euler(0f, yaw, 0f);
                    GameObject prefab = prefabs[rng.Next(0, prefabs.Length)];
                    if (Application.isEditor && !Application.isPlaying)
                    {
#if UNITY_EDITOR
                        GameObject instance = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(prefab, transform);
                        instance.transform.SetPositionAndRotation(worldPosition, rotation);
#endif
                    }
                    else
                    {
                        Instantiate(prefab, worldPosition, rotation, transform);
                    }

                    break;
                }
            }
        }

        Vector3 GetRandomPosition()
        {
            float x = Mathf.Lerp(min.x, max.x, (float)rng.NextDouble());
            float z = Mathf.Lerp(min.y, max.y, (float)rng.NextDouble());
            return new Vector3(x, y, z);
        }

        bool TooClose(Vector2Int position)
        {
            return Array.IndexOf(positions, position, 0, count) != -1;
        }
    }

    private static Vector2Int Compress(Vector3 position, float radius)
    {
        int x = (int)(position.x * radius);
        int z = (int)(position.z * radius);
        return new Vector2Int(x, z);
    }
}
