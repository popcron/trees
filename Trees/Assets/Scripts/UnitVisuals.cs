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
        int layer = isFocalPoint ? focalPointLayer : defaultLayer;
        foreach (PawnPart part in pawn.Parts)
        {
            part.Color = unit.color;
            if (part.renderer.gameObject.layer != layer)
            {
                part.renderer.gameObject.layer = layer;
            }
        }

        foreach (Renderer eyeRenderer in pawn.body.head.leftEye.renderers)
        {
            if (eyeRenderer.gameObject.layer != layer)
            {
                eyeRenderer.gameObject.layer = layer;
            }
        }

        foreach (Renderer eyeRenderer in pawn.body.head.rightEye.renderers)
        {
            if (eyeRenderer.gameObject.layer != layer)
            {
                eyeRenderer.gameObject.layer = layer;
            }
        }

        pawn.eyePitchYaw = unit.eyePitchYaw;
        pawn.headPitchYaw = unit.headPitchYaw;
        pawn.eyeDistance = Mathf.Max(unit.eyeDistance - 0.1f, 0.1f);
        pawn.bodyYaw = unit.bodyYaw;
    }
}
