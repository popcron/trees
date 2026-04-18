using Scripting;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MaterialPropertyBlocks.Property))]
public class MaterialPropertyBlockDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        SerializedProperty name = property.FindPropertyRelative(nameof(MaterialPropertyBlocks.Property.name));
        SerializedProperty typeProperty = property.FindPropertyRelative(nameof(MaterialPropertyBlocks.Property.type));
        MaterialPropertyBlocks.Property.Type type = (MaterialPropertyBlocks.Property.Type)typeProperty.enumValueIndex;

        float spacing = 2f;
        float thirdWidth = (position.width - spacing * 2f) / 3f;
        Rect nameRect = new(position.x, position.y, thirdWidth, position.height);
        Rect typeRect = new(nameRect.xMax + spacing, position.y, thirdWidth, position.height);
        Rect valueRect = new(typeRect.xMax + spacing, position.y, thirdWidth, position.height);

        EditorGUI.PropertyField(nameRect, name, GUIContent.none);
        EditorGUI.PropertyField(typeRect, typeProperty, GUIContent.none);
        if (type == MaterialPropertyBlocks.Property.Type.Int)
        {
            SerializedProperty intValue = property.FindPropertyRelative(nameof(MaterialPropertyBlocks.Property.intValue));
            EditorGUI.PropertyField(valueRect, intValue, GUIContent.none);
        }
        else if (type == MaterialPropertyBlocks.Property.Type.Float)
        {
            SerializedProperty floatValue = property.FindPropertyRelative(nameof(MaterialPropertyBlocks.Property.floatValue));
            EditorGUI.PropertyField(valueRect, floatValue, GUIContent.none);
        }
        else if (type == MaterialPropertyBlocks.Property.Type.Vector4)
        {
            SerializedProperty vectorValue = property.FindPropertyRelative(nameof(MaterialPropertyBlocks.Property.vectorValue));
            EditorGUI.PropertyField(valueRect, vectorValue, GUIContent.none);
        }
        else if (type == MaterialPropertyBlocks.Property.Type.Color)
        {
            SerializedProperty colorValue = property.FindPropertyRelative(nameof(MaterialPropertyBlocks.Property.colorValue));
            EditorGUI.PropertyField(valueRect, colorValue, GUIContent.none);
        }
        else if (type == MaterialPropertyBlocks.Property.Type.Texture)
        {
            SerializedProperty textureValue = property.FindPropertyRelative(nameof(MaterialPropertyBlocks.Property.textureValue));
            EditorGUI.PropertyField(valueRect, textureValue, GUIContent.none);
        }
        else
        {
            EditorGUI.LabelField(valueRect, "Unknown type");
        }

        EditorGUI.EndProperty();
    }
}
