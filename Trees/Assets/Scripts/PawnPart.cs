using UnityEngine;

[ExecuteAlways]
public class PawnPart : BaseBehaviour
{
    public Renderer renderer;
    public MaterialPropertyBlocks blocks;

    public ref Color Color => ref blocks.GetProperty("_BaseColor").colorValue;
}
