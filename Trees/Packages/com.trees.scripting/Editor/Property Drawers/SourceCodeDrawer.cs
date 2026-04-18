using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripting
{
    [CustomPropertyDrawer(typeof(SourceCode))]
    [CustomPropertyDrawer(typeof(SourceCode<>))]
    public class SourceCodeDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            Interpreter interpreter = EditorConstants.GetInterpreter(property);
            Type genericArg = null;
            Type fieldType = fieldInfo.FieldType;
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(SourceCode<>))
            {
                genericArg = fieldType.GetGenericArguments()[0];
            }

            TextField field = new();
            field.multiline = true;
            field.AddToClassList("source-code-field");
            field.BindProperty(property.FindPropertyRelative(nameof(SourceCode.content)));
            field.style.unityFontDefinition = EditorConstants.NoFontAssetDefinition();
            field.style.minHeight = 60;
            field.style.whiteSpace = WhiteSpace.NoWrap;
            field.label = preferredLabel;
            field.AddToClassList(BaseField<string>.alignedFieldUssClassName);
            if (genericArg == null)
            {
                EditorConstants.SetupSourceCodeField(field, interpreter);
                return field;
            }

            VisualElement previewRoot = new();
            previewRoot.style.position = Position.Absolute;
            previewRoot.style.bottom = 2;
            previewRoot.style.right = 4;
            previewRoot.style.flexDirection = FlexDirection.Row;
            previewRoot.style.width = 50;

            // Button editToggle = new();
            // previewRoot.Add(editToggle);
            field.Q(className: "unity-base-text-field__input").Add(previewRoot);

            Func<string, bool> updater = CreateStatusUpdater(interpreter, genericArg, previewRoot, field);
            VisualElement valueDrawer = previewRoot[0];
            valueDrawer.style.flexGrow = 1;
            EditorConstants.SetupSourceCodeField(field, interpreter, updater);
            return field;
        }

        private static Func<string, bool> CreateStatusUpdater(Interpreter interpreter, Type type, VisualElement previewRoot, TextField sourceField)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.String:
                    {
                        Label label = new() { name = "status" };
                        label.style.unityTextAlign = TextAnchor.MiddleRight;
                        label.style.flexGrow = 1;
                        label.style.whiteSpace = WhiteSpace.Normal;
                        previewRoot.Insert(0, label);
                        return MakeUpdater<string>(interpreter, label);
                    }
                case TypeCode.Boolean:
                    {
                        Toggle element = new();
                        RegisterWriteBack(element, interpreter, sourceField);
                        previewRoot.Insert(0, element);
                        return MakeUpdater<bool>(interpreter, element);
                    }
                case TypeCode.Byte:
                    {
                        Label label = new() { name = "status" };
                        previewRoot.Insert(0, label);
                        return MakeUpdater<byte>(interpreter, label);
                    }
                case TypeCode.SByte:
                    {
                        Label label = new() { name = "status" };
                        previewRoot.Insert(0, label);
                        return MakeUpdater<sbyte>(interpreter, label);
                    }
                case TypeCode.Char:
                    {
                        Label label = new() { name = "status" };
                        previewRoot.Insert(0, label);
                        return MakeUpdater<char>(interpreter, label);
                    }
                case TypeCode.Int16:
                    {
                        Label label = new() { name = "status" };
                        previewRoot.Insert(0, label);
                        return MakeUpdater<short>(interpreter, label);
                    }
                case TypeCode.UInt16:
                    {
                        Label label = new() { name = "status" };
                        previewRoot.Insert(0, label);
                        return MakeUpdater<ushort>(interpreter, label);
                    }
                case TypeCode.Int32:
                    {
                        IntegerField element = new();
                        RegisterWriteBack(element, interpreter, sourceField);
                        previewRoot.Insert(0, element);
                        return MakeUpdater<int>(interpreter, element);
                    }
                case TypeCode.UInt32:
                    {
                        Label label = new() { name = "status" };
                        previewRoot.Insert(0, label);
                        return MakeUpdater<uint>(interpreter, label);
                    }
                case TypeCode.Int64:
                    {
                        LongField element = new();
                        RegisterWriteBack(element, interpreter, sourceField);
                        previewRoot.Insert(0, element);
                        return MakeUpdater<long>(interpreter, element);
                    }
                case TypeCode.UInt64:
                    {
                        Label label = new() { name = "status" };
                        previewRoot.Insert(0, label);
                        return MakeUpdater<ulong>(interpreter, label);
                    }
                case TypeCode.Single:
                    {
                        FloatField element = new();
                        RegisterWriteBack(element, interpreter, sourceField);
                        previewRoot.Insert(0, element);
                        return MakeUpdater<float>(interpreter, element);
                    }
                case TypeCode.Double:
                    {
                        DoubleField element = new();
                        RegisterWriteBack(element, interpreter, sourceField);
                        previewRoot.Insert(0, element);
                        return MakeUpdater<double>(interpreter, element);
                    }
                default:
                    {
                        if (type == typeof(Color))
                        {
                            ColorField element = new();
                            RegisterWriteBack(element, interpreter, sourceField);
                            previewRoot.Insert(0, element);
                            return MakeUpdater<Color>(interpreter, element);
                        }
                        else if (type == typeof(Vector2))
                        {
                            Vector2Field element = new();
                            RegisterWriteBack(element, interpreter, sourceField);
                            previewRoot.Insert(0, element);
                            return MakeUpdater<Vector2>(interpreter, element);
                        }
                        else if (type == typeof(Vector3))
                        {
                            Vector3Field element = new();
                            RegisterWriteBack(element, interpreter, sourceField);
                            previewRoot.Insert(0, element);
                            return MakeUpdater<Vector3>(interpreter, element);
                        }

                        Label label = new() { name = "status" };
                        previewRoot.Insert(0, label);
                        if (type == typeof(Vector4))
                        {
                            return MakeUpdater<Vector4>(interpreter, label);
                        }
                        else if (type == typeof(Vector2Int))
                        {
                            return MakeUpdater<Vector2Int>(interpreter, label);
                        }
                        else if (type == typeof(Vector3Int))
                        {
                            return MakeUpdater<Vector3Int>(interpreter, label);
                        }
                        else if (type == typeof(Quaternion))
                        {
                            return MakeUpdater<Quaternion>(interpreter, label);
                        }

                        return null;
                    }
            }
        }

        private static Func<string, bool> MakeUpdater<TValue>(Interpreter interpreter, BaseField<TValue> field)
        {
            return source =>
            {
                try
                {
                    Value value = interpreter.Evaluate(source);
                    if (value.TryDeserialize(out TValue result))
                    {
                        field.SetValueWithoutNotify(result);
                        field.SetEnabled(new SourceCode<TValue>(result).content == source);
                        return true;
                    }

                    field.SetValueWithoutNotify(default);
                    field.SetEnabled(false);
                    return false;
                }
                catch (Exception)
                {
                    field.SetValueWithoutNotify(default);
                    field.SetEnabled(false);
                    return false;
                }
            };
        }

        private static void RegisterWriteBack<TValue>(BaseField<TValue> element, Interpreter interpreter, TextField sourceField)
        {
            element.RegisterValueChangedCallback(evt =>
            {
                string source = sourceField.value;
                try
                {
                    Value evaluated = interpreter.Evaluate(source);
                    if (!evaluated.TryDeserialize(out TValue current)) return;
                    if (new SourceCode<TValue>(current).content != source) return;
                    sourceField.value = new SourceCode<TValue>(evt.newValue).content;
                }
                catch (Exception) { }
            });
        }

        private static Func<string, bool> MakeUpdater<TValue>(Interpreter interpreter, Label status)
        {
            return source =>
            {
                try
                {
                    Value value = interpreter.Evaluate(source);
                    if (value.TryDeserialize(out TValue result))
                    {
                        status.text = result.ToString();
                        return true;
                    }
                    status.text = string.Empty;
                    return false;
                }
                catch (Exception)
                {
                    status.text = string.Empty;
                    return false;
                }
            };
        }
    }
}
