using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ProgramWindow : EditorWindow
{
    private const int FontSize = 14;
    private static Font MonospaceFont;

    private ScrollView scrollView;

    private void OnEnable()
    {
        if (MonospaceFont == null)
        {
            MonospaceFont = Font.CreateDynamicFontFromOSFont("Consolas", FontSize);
        }
    }

    private void CreateGUI()
    {
        scrollView = new ScrollView(ScrollViewMode.Vertical);
        scrollView.style.flexGrow = 1;
        rootVisualElement.Add(scrollView);

        Refresh();
    }

    private void Refresh()
    {
        scrollView.Clear();
        Add("Nothing to see here yet");
    }

    private void Add(ReadOnlySpan<char> text)
    {
        Label label = new(text.ToString());
        label.style.unityFont = MonospaceFont;
        label.style.fontSize = FontSize;
        label.style.unityFontDefinition = new StyleFontDefinition(new FontDefinition { fontAsset = null });
        label.style.paddingLeft = 4;
        label.style.paddingTop = 2;
        label.style.paddingBottom = 2;
        scrollView.Add(label);
    }

    [MenuItem("Window/Program")]
    public static void Open()
    {
        GetWindow<ProgramWindow>("Program");
    }
}