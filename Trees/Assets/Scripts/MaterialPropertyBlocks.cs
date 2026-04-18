using System;
using UnityEngine;

[ExecuteAlways]
public class MaterialPropertyBlocks : MonoBehaviour
{
    public Property[] properties = { };
    public Entry[] renderers = { };

    private void Reset()
    {
        Renderer[] components = GetComponentsInChildren<Renderer>();
        renderers = new Entry[components.Length];
        for (int i = 0; i < components.Length; i++)
        {
            renderers[i] = new Entry(components[i], null);
        }
    }

    private void OnEnable()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            ref Entry entry = ref renderers[i];
            MaterialPropertyBlock block = new();
            entry.block = block;
            entry.renderer.GetPropertyBlock(block);
            SetProperties(block);
            entry.renderer.SetPropertyBlock(block);
        }
    }

    private void Update()
    {
        ApplyAllBlocks();
    }

    private void OnDisable()
    {
        // remove the blocks
        for (int i = 0; i < renderers.Length; i++)
        {
            ref Entry entry = ref renderers[i];
            entry.block.Clear();
            entry.renderer.SetPropertyBlock(entry.block);
        }
    }

    [ContextMenu("Apply All Blocks")]
    public void ApplyAllBlocks()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            ref Entry entry = ref renderers[i];
            if (entry.block == null)
            {
                // happens in the editor
                entry.block = new();
                entry.renderer.GetPropertyBlock(entry.block);
            }

            SetProperties(entry.block);
            entry.renderer.SetPropertyBlock(entry.block);
        }
    }

    public void SetProperties(MaterialPropertyBlock block)
    {
        for (int i = 0; i < properties.Length; i++)
        {
            ref Property property = ref properties[i];
            if (property.type == Property.Type.Int)
            {
                block.SetFloat(property.name, property.intValue);
            }
            else if (property.type == Property.Type.Float)
            {
                block.SetFloat(property.name, property.floatValue);
            }
            else if (property.type == Property.Type.Vector4)
            {
                block.SetVector(property.name, property.vectorValue);
            }
            else if (property.type == Property.Type.Color)
            {
                block.SetColor(property.name, property.colorValue);
            }
            else if (property.type == Property.Type.Texture)
            {
                block.SetTexture(property.name, property.textureValue);
            }
        }
    }

    public ref Property GetProperty(ReadOnlySpan<char> name)
    {
        for (int i = 0; i < properties.Length; i++)
        {
            ref Property property = ref properties[i];
            if (name.SequenceEqual(property.name))
            {
                return ref property;
            }
        }

        throw new Exception($"Property {name.ToString()} not found");
    }

    [Serializable]
    public struct Property
    {
        public string name;
        public Type type;
        public int intValue;
        public float floatValue;
        public Vector4 vectorValue;
        public Color colorValue;
        public Texture2D textureValue;

        public enum Type
        {
            Unknown,
            Int,
            Float,
            Vector4,
            Color,
            Texture
        }
    }

    [Serializable]
    public struct Entry
    {
        public Renderer renderer;

        [NonSerialized]
        public MaterialPropertyBlock block;

        public Entry(Renderer renderer, MaterialPropertyBlock block)
        {
            this.renderer = renderer;
            this.block = block;
        }
    }
}