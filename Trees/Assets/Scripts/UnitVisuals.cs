using UnityEngine;

[ExecuteAlways]
public class UnitVisuals : MonoBehaviour
{
    public Unit unit;
    public Pawn pawn;

    private void Reset()
    {
        unit = GetComponentInParent<Unit>();
        pawn = GetComponentInChildren<Pawn>();
    }

    private void Update()
    {
        bool isFocalPoint = Program.focalPoint == unit;
        int defaultLayer = LayerMask.NameToLayer("Default");
        int focalPointLayer = LayerMask.NameToLayer("Focal Point");
        foreach (PawnPart part in pawn.Parts)
        {
            part.Color = unit.color;
            part.renderer.gameObject.layer = isFocalPoint ? focalPointLayer : defaultLayer;
        }

        foreach (Renderer eyeRenderer in pawn.body.head.leftEye.renderers)
        {
            eyeRenderer.gameObject.layer = isFocalPoint ? focalPointLayer : defaultLayer;
        }

        foreach (Renderer eyeRenderer in pawn.body.head.rightEye.renderers)
        {
            eyeRenderer.gameObject.layer = isFocalPoint ? focalPointLayer : defaultLayer;
        }

        pawn.eyePitchYaw = unit.eyePitchYaw;
        pawn.headPitchYaw = unit.headPitchYaw;
        pawn.eyeDistance = Mathf.Max(unit.eyeDistance - 0.1f, 0.1f);
        pawn.bodyYaw = unit.bodyYaw;
    }
}
