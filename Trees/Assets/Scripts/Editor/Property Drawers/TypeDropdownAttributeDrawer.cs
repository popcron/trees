using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(TypeDropdownAttribute))]
public class TypeDropdownAttributeDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        TypeDropdownAttribute typeDropdown = (TypeDropdownAttribute)attribute;
        Type deriveFrom = typeDropdown.deriveFrom;

        SerializedProperty value = property;
        Type[] allOptions = deriveFrom == null ? AllTypes.array : EditorTypeTable.GetTypesDerivedFrom(deriveFrom);

        VisualElement container = new();
        container.style.flexDirection = FlexDirection.Row;
        container.style.alignItems = Align.Center;

        Label label = new(property.displayName);
        label.style.width = new Length(EditorGUIUtility.labelWidth, LengthUnit.Pixel);
        label.style.flexShrink = 0;

        Button button = new();
        button.style.flexGrow = 1;
        button.style.minHeight = new Length(20, LengthUnit.Pixel);
        button.style.unityTextAlign = TextAnchor.MiddleLeft;
        RefreshButtonLabel(button, allOptions, value);

        button.clicked += () =>
        {
            TypeSearchDropdown dropdown = new(new AdvancedDropdownState(), allOptions, selectedType =>
            {
                if (selectedType == null)
                {
                    value.ulongValue = 0;
                }
                else
                {
                    ulong hash = selectedType.GetID();
                    value.ulongValue = hash;
                    TypeExtensions.types[hash] = selectedType;
                }

                value.serializedObject.ApplyModifiedProperties();
                RefreshButtonLabel(button, allOptions, value);
            }, deriveFrom);
            dropdown.Show(button.worldBound);
        };

        container.Add(label);
        container.Add(button);
        return container;
    }

    public static void RefreshButtonLabel(Button button, Type[] allOptions, SerializedProperty value)
    {
        Type currentType = GetTypeFromHash(allOptions, value.ulongValue);
        button.text = currentType == null ? "None" : GetPrettyTypeName(currentType);
    }

    public static string GetPrettyTypeName(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        int backtickIndex = type.Name.LastIndexOf('`');
        string baseName = type.Name.Substring(0, backtickIndex);
        Type[] genericArgs = type.GetGenericArguments();
        string[] argNames = new string[genericArgs.Length];
        for (int i = 0; i < genericArgs.Length; i++)
        {
            argNames[i] = GetPrettyTypeName(genericArgs[i]);
        }

        return string.Format("{0}<{1}>", baseName, string.Join(", ", argNames));
    }

    public static Type GetTypeFromHash(Type[] options, ulong hash)
    {
        for (int i = 0; i < options.Length; i++)
        {
            Type type = options[i];
            if (type.GetID() == hash)
            {
                TypeExtensions.types[hash] = type;
                return type;
            }
        }

        return null;
    }

    public class TypeSearchDropdown : AdvancedDropdown
    {
        public Type[] types;
        public Action<Type> onSelected;
        public Type filterType;

        public TypeSearchDropdown(AdvancedDropdownState state, Type[] types, Action<Type> onSelected, Type filterType = null) : base(state)
        {
            this.types = types;
            this.onSelected = onSelected;
            this.filterType = filterType;
            minimumSize = new Vector2(300, 400);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            string rootTitle = filterType != null ? $"Type (derives from {GetPrettyTypeName(filterType)})" : "Type";
            AdvancedDropdownItem root = new(rootTitle);
            root.AddChild(new TypeDropdownItem("None", null));

            HashSet<Type> allTypesSet = new(types);
            HashSet<Type> processed = new();
            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                Type baseType = type.BaseType;
                if (baseType == null || !allTypesSet.Contains(baseType))
                {
                    AddTypeHierarchy(root, type, allTypesSet, processed);
                }
            }

            return root;
        }

        public void AddTypeHierarchy(AdvancedDropdownItem parentItem, Type type, HashSet<Type> allTypesSet, HashSet<Type> processed)
        {
            if (processed.Contains(type))
            {
                return;
            }

            processed.Add(type);
            AdvancedDropdownItem typeItem = new TypeDropdownItem(GetPrettyTypeName(type), type);
            parentItem.AddChild(typeItem);
            for (int i = 0; i < types.Length; i++)
            {
                Type candidate = types[i];
                if (candidate.BaseType == type && !processed.Contains(candidate))
                {
                    AddTypeHierarchy(typeItem, candidate, allTypesSet, processed);
                }
            }
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            TypeDropdownItem typeItem = (TypeDropdownItem)item;
            onSelected(typeItem.type);
        }
    }

    public class TypeDropdownItem : AdvancedDropdownItem
    {
        public Type type;

        public TypeDropdownItem(string name, Type type) : base(name)
        {
            this.type = type;
        }
    }

}