using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Pawn)), CanEditMultipleObjects]
public class PawnEditor : Editor
{
    protected virtual void OnSceneGUI()
    {
        Pawn pawn = (Pawn)target;
        Quaternion eyeDirection = Quaternion.Euler(pawn.eyePitchYaw.x, pawn.eyePitchYaw.y, 0f);
        Vector3 direction = eyeDirection * Vector3.forward;
        Vector3 eyeTargetPosition = pawn.body.head.transform.position + direction * pawn.eyeDistance;
        EditorGUI.BeginChangeCheck();
        Vector3 newTargetPosition = Handles.PositionHandle(eyeTargetPosition, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(pawn, "Change Eye Look Direction");
            pawn.StareAt(newTargetPosition);
        }
    }
}
