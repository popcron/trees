using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripting
{
    [CustomPropertyDrawer(typeof(ScriptBehaviour.ObjectBinding))]
    public class ObjectBindingDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElement container = new();
            SerializedProperty name = property.FindPropertyRelative(nameof(ScriptBehaviour.ObjectBinding.name));
            SerializedProperty target = property.FindPropertyRelative(nameof(ScriptBehaviour.ObjectBinding.target));
            SerializedProperty member = property.FindPropertyRelative(nameof(ScriptBehaviour.ObjectBinding.member));

            TextField nameField = new(name.displayName);
            nameField.AddToClassList(BaseField<string>.alignedFieldUssClassName);
            nameField.BindProperty(name);
            nameField.style.unityFontDefinition = new StyleFontDefinition(new FontDefinition { fontAsset = null });
            ColorUtility.TryParseHtmlString(SyntaxColors.BindingColor, out Color bindingColor);
            nameField.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                TextElement input = nameField.Q<TextElement>(className: "unity-text-element--inner-input-field-component");
                if (input == null)
                {
                    return;
                }

                input.style.unityFont = EditorConstants.Font;
                input.style.fontSize = EditorConstants.FontSize;
                input.style.color = new StyleColor(bindingColor);
            });

            container.Add(nameField);
            container.Add(new PropertyField(target));
            container.Add(new PropertyField(member));

            // todo: need to show a checkmark and what member it resolved to
            return container;
        }
    }
}
