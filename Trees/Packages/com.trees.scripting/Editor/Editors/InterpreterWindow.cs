using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UIElements.VisualElement;

namespace Scripting
{
    public class InterpreterWindow : EditorWindow
    {
        private TextField inputField;
        private TextField outputField;
        private ScrollView bindingsView;
        private readonly List<(Range range, Color color)> diagnostics = new();
        private VisualElement squiggleLayer;

        private void CreateGUI()
        {
            rootVisualElement.style.flexDirection = FlexDirection.Column;
            rootVisualElement.style.flexGrow = 1;

            VisualElement topRow = new();
            topRow.style.flexGrow = 1;
            topRow.style.flexBasis = 0;
            topRow.style.flexDirection = FlexDirection.Row;

            inputField = new TextField();
            inputField.multiline = true;
            inputField.style.flexGrow = 1;
            inputField.style.flexBasis = 0;
            inputField.style.unityFontDefinition = EditorConstants.NoFontAssetDefinition();
            inputField.value = EditorConstants.StoredSourceCode;

            Label inputOverlay = new()
            {
                pickingMode = PickingMode.Ignore,
                enableRichText = true
            };

            inputOverlay.RegisterCallback<ChangeEvent<string>>(e => e.StopImmediatePropagation());
            inputField.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                TextElement input = EditorConstants.ConfigureSourceCodeInput(inputField);
                if (input == null)
                {
                    return;
                }

                input.enableRichText = false;
                VisualElement inputParent = input.parent;
                inputParent.Add(inputOverlay);
                EditorConstants.ConfigureOverlayLabel(input, inputOverlay);
                EditorConstants.HighlightInto(ScriptingLibrary.interpreter, inputField.value, inputOverlay);

                squiggleLayer = new VisualElement { pickingMode = PickingMode.Ignore };
                EditorConstants.PositionAsInputOverlay(squiggleLayer, input);
                inputParent.Add(squiggleLayer);

                Vector2 cell = inputOverlay.MeasureTextSize("M", 0, MeasureMode.Undefined, 0, MeasureMode.Undefined);
                float charW = cell.x;
                float lineH = cell.y;

                squiggleLayer.generateVisualContent += ctx =>
                {
                    if (diagnostics.Count == 0)
                    {
                        return;
                    }

                    Painter2D p = ctx.painter2D;
                    p.lineWidth = 1f;
                    string text = inputField.value;

                    foreach ((Range range, Color color) in diagnostics)
                    {
                        int start = range.Start.Value;
                        int end = range.End.Value;
                        int length = end - start;
                        int line = 0;
                        int col = 0;
                        for (int i = 0; i < start && i < text.Length; i++)
                        {
                            if (text[i] == '\n')
                            {
                                line++;
                                col = 0;
                            }
                            else
                            {
                                col++;
                            }
                        }

                        float x0 = col * charW;
                        float x1 = x0 + length * charW;
                        float y = (line + 1) * lineH - 1.5f;
                        p.strokeColor = color;
                        p.BeginPath();
                        p.MoveTo(new Vector2(x0, y));
                        const float amp = 1.5f;
                        const float period = 4f;
                        for (float x = x0; x <= x1; x += 1f)
                        {
                            float yy = y + Mathf.Sin((x - x0) * (Mathf.PI * 2f / period)) * amp;
                            p.LineTo(new Vector2(x, yy));
                        }

                        p.Stroke();
                    }
                };
            });

            inputField.RegisterValueChangedCallback(evt =>
            {
                EditorConstants.StoredSourceCode = evt.newValue;
                EditorConstants.HighlightInto(ScriptingLibrary.interpreter, inputField.value, inputOverlay);
            });

            inputField.RegisterCallback<KeyDownEvent>(evt =>
            {
                EditorConstants.HandleSourceCodeEditorKeys(inputField, evt);
            }, TrickleDown.TrickleDown);

            bindingsView = new ScrollView(ScrollViewMode.Vertical);
            bindingsView.style.flexGrow = 1;

            VisualElement leftColumn = new();
            leftColumn.style.flexGrow = 0;
            leftColumn.style.flexShrink = 0;
            leftColumn.style.width = 200;
            leftColumn.style.flexDirection = FlexDirection.Column;
            leftColumn.Add(CreateHeader("Bindings"));
            leftColumn.Add(bindingsView);

            VisualElement rightColumn = new();
            rightColumn.style.flexGrow = 1;
            rightColumn.style.flexBasis = 0;
            rightColumn.style.flexDirection = FlexDirection.Column;
            rightColumn.Add(CreateHeader("Input"));
            rightColumn.Add(inputField);

            topRow.Add(leftColumn);
            topRow.Add(rightColumn);

            // output
            VisualElement outputSection = new();
            outputSection.style.flexGrow = 1;
            outputSection.style.flexBasis = 0;
            outputSection.style.flexDirection = FlexDirection.Column;
            outputSection.Add(CreateHeader("Output"));

            outputField = new TextField();
            outputField.multiline = true;
            outputField.isReadOnly = true;
            outputField.value = "No captured output yet";
            outputField.style.flexGrow = 1;
            outputField.style.flexBasis = 0;
            outputField.style.overflow = Overflow.Hidden;
            outputField.style.unityFontDefinition = EditorConstants.NoFontAssetDefinition();
            outputField.style.backgroundColor = new Color(0f, 0f, 0f, 0.15f);

            outputField.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                TextElement output = EditorConstants.FindInnerTextInput(outputField);
                if (output == null)
                {
                    return;
                }

                output.style.unityFont = EditorConstants.Font;
                output.style.fontSize = EditorConstants.FontSize;
                output.style.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            });

            // footer
            VisualElement footer = new();
            footer.style.flexShrink = 0;
            Button evaluateButton = new(OnEvaluate) { text = "Evaluate (Ctrl+Enter)" };
            footer.Add(evaluateButton);

            outputSection.Add(outputField);

            rootVisualElement.Add(topRow);
            rootVisualElement.Add(outputSection);
            rootVisualElement.Add(footer);

            rootVisualElement.RegisterCallback<KeyDownEvent>(evt =>
            {
                if (evt.keyCode == KeyCode.Return && evt.ctrlKey)
                {
                    OnEvaluate();
                    evt.StopPropagation();
                }
            });

            RefreshBindings();
        }

        private static Label CreateHeader(string text)
        {
            Label header = new(text);
            header.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.style.paddingLeft = 4;
            header.style.paddingTop = 2;
            header.style.paddingBottom = 2;
            header.style.flexShrink = 0;
            return header;
        }

        private void RefreshBindings()
        {
            bindingsView.Clear();
            foreach (Interpreter.Binding binding in ScriptingLibrary.interpreter.Bindings)
            {
                Label label = new($"<color={SyntaxColors.BindingColor}>{binding.name}</color> = {binding.read()}");
                label.enableRichText = true;
                EditorConstants.ApplyMonospaceFont(label.style);
                label.style.paddingLeft = 4;
                label.style.paddingTop = 2;
                label.style.paddingBottom = 2;
                bindingsView.Add(label);
            }
        }

        private void OnEvaluate()
        {
            ReadOnlySpan<char> sourceCode = inputField.value;
            TextElement outputText = EditorConstants.FindInnerTextInput(outputField);
            try
            {
                Value output = ScriptingLibrary.interpreter.Evaluate(sourceCode);
                outputField.value = output.ToString();
                outputField.style.backgroundColor = new Color(0f, 0f, 0f, 0.15f);
                if (outputText != null)
                {
                    outputText.style.color = Color.white;
                }

                diagnostics.Clear();
                squiggleLayer?.MarkDirtyRepaint();
            }
            catch (Exception ex)
            {
                outputField.value = ex.Message;
                outputField.style.backgroundColor = new Color(0.5f, 0f, 0f, 0.25f);
                if (outputText != null)
                {
                    outputText.style.color = new Color(1f, 0.85f, 0.85f, 1f);
                }

                diagnostics.Clear();
                if (ex is ParserException parserException)
                {
                    int tokenIndex = parserException.tokenIndex;
                    if (tokenIndex < parserException.tokens.Count)
                    {
                        Token token = parserException.tokens[tokenIndex];
                        diagnostics.Add((token.range, Color.red));
                    }
                    else
                    {
                        int length = inputField.value.Length;
                        Range range = new(length - 1, length);
                        diagnostics.Add((range, Color.red));
                    }
                }
                else if (ex is InterpreterException interpreterException)
                {
                    Node node = interpreterException.node;
                    diagnostics.Add((node.range, Color.yellow));
                }

                squiggleLayer?.MarkDirtyRepaint();
            }

            RefreshBindings();
        }

        [MenuItem("Window/Interpreter")]
        public static void Open()
        {
            GetWindow<InterpreterWindow>("Interpreter");
        }
    }
}