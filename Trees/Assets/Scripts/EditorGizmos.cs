using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class EditorGizmos
{
    public static Color color
    {
        get
        {
#if UNITY_EDITOR
            return Handles.color;
#else
            return Color.white;
#endif
        }
        set
        {
#if UNITY_EDITOR
            Handles.color = value;
            Gizmos.color = value;
#endif
        }
    }

    public static void DrawRay(Ray ray, float distance, float thickness = 1f)
    {
        DrawRay(ray.origin, ray.direction * distance, thickness);
    }

    public static void DrawRay(Vector3 from, Vector3 direction, float thickness = 1f)
    {
#if UNITY_EDITOR
        Handles.DrawLine(from, from + direction, thickness);
#endif
    }

    public static void DrawLine(Vector3 from, Vector3 to, float thickness = 1f)
    {
#if UNITY_EDITOR
        Handles.DrawLine(from, to, thickness);
#endif
    }

    public static void ArrowHandleCap(int controlId, Vector3 position, Quaternion rotation, float size, EventType eventType = EventType.Repaint)
    {
#if UNITY_EDITOR
        Handles.ArrowHandleCap(controlId, position, rotation, size, eventType);
#endif
    }

    public static void ArrowHandleCap(int controlId, Ray ray, float distance, float size, EventType eventType = EventType.Repaint)
    {
        ArrowHandleCap(controlId, ray.origin + ray.direction * distance, Quaternion.LookRotation(ray.direction), size, eventType);
    }

    public static void SphereHandleCap(int controlId, Vector3 position, Quaternion rotation, float size, EventType eventType = EventType.Repaint)
    {
#if UNITY_EDITOR
        Handles.SphereHandleCap(controlId, position, rotation, size, eventType);
#endif
    }

    public static void ConeHandleCap(int controlId, Ray ray, float distance, float size, EventType eventType = EventType.Repaint)
    {
        ConeHandleCap(controlId, ray.origin + ray.direction * distance, Quaternion.LookRotation(ray.direction), size, eventType);
    }

    public static void ConeHandleCap(int controlId, Vector3 position, Quaternion rotation, float size, EventType eventType = EventType.Repaint)
    {
#if UNITY_EDITOR
        Handles.ConeHandleCap(controlId, position, rotation, size, eventType);
#endif
    }

    public static Vector3 PositionHandle(Vector3 position, Quaternion rotation)
    {
#if UNITY_EDITOR
        position = Handles.PositionHandle(position, rotation);
#endif
        return position;
    }

    public static Quaternion RotationHandle(Quaternion rotation, Vector3 position)
    {
#if UNITY_EDITOR
        rotation = Handles.RotationHandle(rotation, position);
#endif
        return rotation;
    }

    public static void DrawWireArc(Vector3 center, Vector3 normal, Vector3 from, float angle, float radius, float thickness = 1f)
    {
#if UNITY_EDITOR
        Handles.DrawWireArc(center, normal, from, angle, radius, thickness);
#endif
    }

    public static void DrawWireDisc(Vector3 center, Vector3 normal, float radius, float thickness = 1f)
    {
#if UNITY_EDITOR
        Handles.DrawWireDisc(center, normal, radius, thickness);
#endif
    }

    public static void DrawWireCube(Vector3 center, Vector3 size)
    {
#if UNITY_EDITOR
        Handles.DrawWireCube(center, size);
#endif
    }

    public static void DrawSphere(Vector3 center, float radius)
    {
        Gizmos.DrawSphere(center, radius);
    }
}