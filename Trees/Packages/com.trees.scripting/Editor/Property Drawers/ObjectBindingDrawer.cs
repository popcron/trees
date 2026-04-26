using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripting
{
    [CustomPropertyDrawer(typeof(ScriptBehaviour.ObjectBinding))]
    public class ObjectBindingDrawer : PropertyDrawer
    {
        private const BindingFlags ReflectionFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;

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

            VisualElement memberRow = new();
            memberRow.style.flexDirection = FlexDirection.Row;
            memberRow.style.alignItems = Align.Center;
            memberRow.style.marginTop = 2;
            memberRow.AddToClassList(BaseField<string>.alignedFieldUssClassName);

            Label memberLabel = new(member.displayName);
            memberLabel.AddToClassList("unity-base-field__label");
            memberLabel.style.flexShrink = 0;

            Button memberButton = new();
            memberButton.style.flexGrow = 1;
            memberButton.style.minHeight = new Length(20, LengthUnit.Pixel);
            memberButton.style.unityTextAlign = TextAnchor.MiddleLeft;
            memberButton.style.overflow = Overflow.Hidden;

            Label hint = new() { pickingMode = PickingMode.Ignore };
            hint.style.position = Position.Absolute;
            hint.style.right = 4;
            hint.style.top = 0;
            hint.style.bottom = 0;
            hint.style.fontSize = 10;
            hint.style.color = new Color(0.7f, 0.7f, 0.7f, 0.7f);
            hint.style.unityTextAlign = TextAnchor.MiddleRight;
            memberButton.Add(hint);

            memberButton.clicked += () =>
            {
                UnityEngine.Object targetObj = target.objectReferenceValue;
                if (targetObj == null)
                {
                    return;
                }

                BuildMemberMenu(targetObj.GetType(), target, member, memberButton, hint).DropDown(memberButton.worldBound, memberButton, false);
            };

            memberButton.RegisterCallback<AttachToPanelEvent>(_ => UpdateButton(memberButton, hint, target, member));
            container.TrackPropertyValue(target, _ => UpdateButton(memberButton, hint, target, member));
            container.TrackPropertyValue(member, _ => UpdateButton(memberButton, hint, target, member));

            memberRow.Add(memberLabel);
            memberRow.Add(memberButton);
            container.Add(memberRow);
            return container;
        }

        private static void UpdateButton(Button button, Label hint, SerializedProperty target, SerializedProperty member)
        {
            UnityEngine.Object targetObj = target.objectReferenceValue;
            string memberName = member.stringValue;
            button.SetEnabled(targetObj != null);

            if (targetObj == null || string.IsNullOrEmpty(memberName))
            {
                button.text = "None";
                hint.text = string.Empty;
                return;
            }

            button.text = memberName;
            Type type = targetObj.GetType();

            FieldInfo field = type.GetField(memberName, ReflectionFlags);
            if (field != null)
            {
                hint.text = $"field  {field.FieldType.Name}";
                return;
            }

            PropertyInfo prop = type.GetProperty(memberName, ReflectionFlags);
            if (prop != null)
            {
                hint.text = $"property  {prop.PropertyType.Name}";
                return;
            }

            hint.text = string.Empty;
        }

        private static GenericDropdownMenu BuildMemberMenu(Type type, SerializedProperty target, SerializedProperty member, Button memberButton, Label hint)
        {
            GenericDropdownMenu menu = new();
            string current = member.stringValue;

            Action<string> apply = selected =>
            {
                member.stringValue = selected ?? string.Empty;
                member.serializedObject.ApplyModifiedProperties();
                UpdateButton(memberButton, hint, target, member);
            };

            menu.AddItem("None", string.IsNullOrEmpty(current), () => apply(null));

            HashSet<string> seen = new();
            FieldInfo[] fields = type.GetFields(PublicInstance);
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo f = fields[i];
                if (!seen.Add(f.Name))
                {
                    continue;
                }
                string captured = f.Name;
                menu.AddItem($"Fields/{f.Name}  ({f.FieldType.Name})", current == captured, () => apply(captured));
            }

            PropertyInfo[] props = type.GetProperties(PublicInstance);
            for (int i = 0; i < props.Length; i++)
            {
                PropertyInfo p = props[i];
                if (p.GetGetMethod() == null || p.GetIndexParameters().Length > 0)
                {
                    continue;
                }
                if (!seen.Add(p.Name))
                {
                    continue;
                }
                string captured = p.Name;
                menu.AddItem($"Properties/{p.Name}  ({p.PropertyType.Name})", current == captured, () => apply(captured));
            }

            return menu;
        }
    }
}
