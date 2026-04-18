using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu]
public class GradientAsset : ScriptableObject
{
    public const int Width = 512;
    public const int Height = 8;

    public Gradient gradient = new();
    public Texture2D texture;

    [ContextMenu("Update Texture")]
    public void UpdateTexture()
    {
        if (texture == null)
        {
            texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false)
            {
                name = name,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
        }
        else if (texture.width != Width || texture.height != Height)
        {
            texture.Reinitialize(Width, Height);
        }
        else if (texture.name != name)
        {
            texture.name = name;
        }

        Color32[] pixels = new Color32[Width * Height];
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                pixels[y * Width + x] = gradient.Evaluate(x / (float)(Width - 1));
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();

#if UNITY_EDITOR
        if (!AssetDatabase.Contains(texture))
        {
            AssetDatabase.AddObjectToAsset(texture, this);
        }

        EditorUtility.SetDirty(texture);
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
#endif
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        EditorApplication.delayCall += () =>
        {
            if (this != null)
            {
                UpdateTexture();
            }
        };
    }
#endif
}
