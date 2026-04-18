using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Scripting
{
    [UnityEditor.CustomPropertyDrawer(typeof(SourceCodeAttribute))]
    public class SourceCodeAttributeDrawer : UnityEditor.PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(UnityEditor.SerializedProperty property)
        {
            VisualElement container = new();
            Interpreter interpreter = EditorConstants.GetInterpreter(property);
            TextField field = new(preferredLabel);
            field.AddToClassList("source-code-field");
            field.AddToClassList(BaseField<string>.alignedFieldUssClassName);
            field.BindProperty(property);
            field.style.unityFontDefinition = EditorConstants.NoFontAssetDefinition();
            container.Add(field);
            EditorConstants.SetupSourceCodeField(field, interpreter);
            return container;
        }
    }
}
