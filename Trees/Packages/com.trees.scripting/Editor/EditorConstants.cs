using System;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Scripting
{
    internal static class EditorConstants
    {
        public const int FontSize = 13;
        public const int IndentWidth = 4;
        public const string SourceCodePrefKey = "InterpreterWindow.SourceCode";
        public const string TextInputClassName = "unity-text-element--inner-input-field-component";

        private const string IndentString = "    ";

        private static readonly StringBuilder stringBuilder = new();
        private static Font monospaceFont;

        public static Font Font
        {
            get
            {
                if (monospaceFont == null)
                {
                    monospaceFont = Font.CreateDynamicFontFromOSFont("Consolas", FontSize);
                }

                return monospaceFont;
            }
        }

        public static string StoredSourceCode
        {
            get => PlayerPrefs.GetString(SourceCodePrefKey, string.Empty);
            set => PlayerPrefs.SetString(SourceCodePrefKey, value);
        }

        public static Interpreter GetInterpreter(SerializedProperty property)
        {
            UnityEngine.Object target = property.serializedObject.targetObject;
            if (target is IContainsInterpreter containsInterpreter)
            {
                return containsInterpreter.Interpreter;
            }

            return ScriptingLibrary.interpreter;
        }

        public static StyleFontDefinition NoFontAssetDefinition()
        {
            return new StyleFontDefinition(new FontDefinition { fontAsset = null });
        }

        public static void ApplyMonospaceFont(IStyle style)
        {
            style.unityFont = Font;
            style.fontSize = FontSize;
            style.unityFontDefinition = NoFontAssetDefinition();
        }

        public static TextElement FindInnerTextInput(VisualElement element)
        {
            return element.Q<TextElement>(className: TextInputClassName);
        }

        public static TextElement ConfigureSourceCodeInput(TextField field)
        {
            TextElement input = FindInnerTextInput(field);
            if (input == null)
            {
                return null;
            }

            input.style.unityFont = Font;
            input.style.fontSize = FontSize;
            input.style.whiteSpace = WhiteSpace.NoWrap;
            input.style.color = new Color(0, 0, 0, 0);
            return input;
        }

        public static void PositionAsInputOverlay(VisualElement overlay, VisualElement input)
        {
            overlay.style.position = Position.Absolute;
            overlay.style.left = input.style.paddingLeft;
            overlay.style.top = input.style.paddingTop;
            overlay.style.right = input.style.paddingRight;
            overlay.style.bottom = input.style.paddingBottom;
        }

        public static void ConfigureOverlayLabel(VisualElement input, Label overlay)
        {
            PositionAsInputOverlay(overlay, input);
            overlay.style.unityFont = Font;
            overlay.style.fontSize = FontSize;
            overlay.style.whiteSpace = WhiteSpace.Pre;
            overlay.style.unityFontDefinition = NoFontAssetDefinition();
        }

        public static void ConfigureErrorLabel(Label errorLabel)
        {
            errorLabel.style.position = Position.Absolute;
            errorLabel.style.right = 4;
            errorLabel.style.bottom = 4;
            errorLabel.style.fontSize = 10;
            errorLabel.style.color = new Color(1f, 0.3f, 0.3f, 1f);
            errorLabel.style.unityTextAlign = TextAnchor.LowerRight;
            errorLabel.style.overflow = Overflow.Hidden;
            errorLabel.text = string.Empty;
        }

        public static void HighlightInto(Interpreter interpreter, string source, Label overlay)
        {
            SyntaxHighlighter.Highlight(interpreter, source, stringBuilder);
            overlay.text = stringBuilder.ToString();
            stringBuilder.Clear();
        }

        public static void SetupSourceCodeField(TextField field, Interpreter interpreter, Func<string, bool> updatePreview = null)
        {
            Label overlay = new() { pickingMode = PickingMode.Ignore, enableRichText = true };
            Label errorLabel = new() { pickingMode = PickingMode.Ignore, enableRichText = false };

            double lastKeypressTime = double.MinValue;
            field.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                TextElement input = ConfigureSourceCodeInput(field);
                if (input == null)
                {
                    return;
                }

                VisualElement inputParent = input.parent;
                inputParent.Add(overlay);
                ConfigureOverlayLabel(input, overlay);
                inputParent.Add(errorLabel);
                ConfigureErrorLabel(errorLabel);

                HighlightInto(interpreter, field.value, overlay);
                updatePreview?.Invoke(field.value);

                string lastHighlighted = field.value;
                field.schedule.Execute(() =>
                {
                    if (EditorApplication.timeSinceStartup - lastKeypressTime < 0.1)
                    {
                        return;
                    }

                    if (field.value == lastHighlighted)
                    {
                        return;
                    }

                    lastHighlighted = field.value;
                    HighlightInto(interpreter, field.value, overlay);
                    updatePreview?.Invoke(field.value);
                }).Every(16);
            });

            field.RegisterCallback<KeyDownEvent>(evt =>
            {
                lastKeypressTime = EditorApplication.timeSinceStartup;
                HandleSourceCodeEditorKeys(field, evt);
            }, TrickleDown.TrickleDown);
        }

        public static void HandleSourceCodeEditorKeys(TextField field, KeyDownEvent evt)
        {
            if (evt.keyCode == KeyCode.Tab)
            {
                if (evt.shiftKey)
                {
                    int cursor = field.cursorIndex;
                    string text = field.value;
                    int spaces = 0;
                    while (spaces < IndentWidth && cursor - spaces - 1 >= 0 && text[cursor - spaces - 1] == ' ')
                    {
                        spaces++;
                    }

                    if (spaces > 0)
                    {
                        field.value = text.Remove(cursor - spaces, spaces);
                        field.SelectRange(cursor - spaces, cursor - spaces);
                    }
                }
                else
                {
                    int cursor = field.cursorIndex;
                    field.value = field.value.Insert(cursor, IndentString);
                    field.SelectRange(cursor + IndentWidth, cursor + IndentWidth);
                }

                evt.StopPropagation();
            }
            else if (evt.keyCode == KeyCode.Home)
            {
                string text = field.value;
                int cursor = field.cursorIndex;
                int lineStart = cursor > 0 ? text.LastIndexOf('\n', cursor - 1) : -1;
                lineStart = lineStart == -1 ? 0 : lineStart + 1;
                int anchor = evt.shiftKey ? field.selectIndex : lineStart;
                field.SelectRange(lineStart, anchor);
                evt.StopPropagation();
            }
            else if (evt.keyCode == KeyCode.End)
            {
                string text = field.value;
                int cursor = field.cursorIndex;
                int lineEnd = text.IndexOf('\n', cursor);
                if (lineEnd == -1)
                {
                    lineEnd = text.Length;
                }

                int anchor = evt.shiftKey ? field.selectIndex : lineEnd;
                field.SelectRange(lineEnd, anchor);
                evt.StopPropagation();
            }
        }
    }
}
