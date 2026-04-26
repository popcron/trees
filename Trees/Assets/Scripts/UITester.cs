using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

[ExecuteAlways]
public class UITester : MonoBehaviour
{
    public Color backgroundColor = Color.red;
    public Color fontColor = Color.white;
    public AnimationCurve fontSize = AnimationCurve.Linear(0f, 24f, 1f, 24f);
    public float animationSpeed = 1f;
    public PanelSettings panelSettings;
    public ThemeStyleSheet themeStyleSheet;
    public IPanel panel;
    public VisualElement gui;

    private void OnEnable()
    {
        panelSettings = ScriptableObject.CreateInstance<PanelSettings>();
        panelSettings.targetTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
        panelSettings.targetTexture.Create();
        ThanksUnity.Create(panelSettings);                                      // panelSettings.Create()
        panel = ThanksUnity.GetPanel(panelSettings);                            // panelSettings.panel
        ThanksUnity.SetTargetTexture(panel, panelSettings.targetTexture);       // panel.targetTexture = panelSettings.targetTexture
        panelSettings.themeStyleSheet = themeStyleSheet;                        // also calls panelSettings.ApplyThemeStyleSheet(), so even if null this is needed
        gui = CreateGUI();
        panel.visualTree.Add(gui);
        panel.visualTree.MarkDirtyRepaint();
    }

    private void OnDisable()
    {
        ThanksUnity.DisposePanel(panelSettings);

        if (Application.isPlaying)
        {
            Destroy(panelSettings);
        }
        else
        {
            DestroyImmediate(panelSettings);
        }

        panelSettings = null;
        panel = null;
        gui = null;
    }

    private void LateUpdate()
    {
        ClearRenderTextureBeforeRendering();
    }

    private void OnGUI()
    {
        DrawRenderTexture();
    }

    private void Update()
    {
        UpdateGUI();
    }

    public VisualElement CreateGUI()
    {
        VisualElement root = new();
        root.style.position = Position.Absolute;
        root.style.backgroundColor = backgroundColor;

        Label label = new Label("Hello from UIToolKit!");
        label.style.fontSize = fontSize.Evaluate(0f);
        label.style.color = fontColor;
        label.style.unityTextAlign = TextAnchor.MiddleCenter;
        label.style.flexGrow = 1;

        // panel settings needs a font set and thats too much just for this example
        Font builtinFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.style.unityFontDefinition = FontDefinition.FromFont(builtinFont);

        root.Add(label);
        return root;
    }

    public void UpdateGUI()
    {
        ResizeTargetTextureIfNeeded();

        float t = (Time.time * animationSpeed % 1f);
        gui.style.translate = new Translate(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
        gui.style.rotate = new Rotate(new Angle(transform.localEulerAngles.z, AngleUnit.Degree));
        gui.style.width = transform.localScale.x;
        gui.style.height = transform.localScale.y;
        gui.style.backgroundColor = backgroundColor;

        Label label = gui.Q<Label>();
        label.style.fontSize = fontSize.Evaluate(t);
        label.style.color = fontColor;
    }

    private void ResizeTargetTextureIfNeeded()
    {
        if (panelSettings.targetTexture.width != Screen.width || panelSettings.targetTexture.height != Screen.height)
        {
            panelSettings.targetTexture.Release();
            panelSettings.targetTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.ARGB32);
            panelSettings.targetTexture.Create();
            ThanksUnity.SetTargetTexture(panel, panelSettings.targetTexture);
        }
    }

    public void ClearRenderTextureBeforeRendering()
    {
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture.active = panelSettings.targetTexture;
        GL.Clear(true, true, Color.clear);
        RenderTexture.active = previousActive;
    }

    public void DrawRenderTexture()
    {
        RenderTexture targetTexture = panelSettings.targetTexture;
        Rect rect = new(0f, 0f, targetTexture.width, targetTexture.height);
        GUI.DrawTexture(rect, targetTexture);
    }

    public static class ThanksUnity
    {
        private const BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly Type panelSettingsType = typeof(PanelSettings);
        private static readonly Assembly uiElementsAssembly = panelSettingsType.Assembly;
        private static readonly Type runtimePanelType = uiElementsAssembly.GetType("UnityEngine.UIElements.RuntimePanel");
        private static readonly MethodInfo createMethod = runtimePanelType.GetMethod("Create", Static);
        private static readonly PropertyInfo panelProperty = panelSettingsType.GetProperty("panel", Instance);
        private static readonly Type baseRuntimePanelType = runtimePanelType.BaseType;
        private static readonly FieldInfo targetTextureField = baseRuntimePanelType.GetField("targetTexture", Instance);
        private static readonly MethodInfo disposePanel = panelSettingsType.GetMethod("DisposePanel", Instance);

        public static void Create(PanelSettings panelSettings)
        {
            createMethod.Invoke(null, new object[] { panelSettings });
        }

        public static IPanel GetPanel(PanelSettings panelSettings)
        {
            return (IPanel)panelProperty.GetValue(panelSettings);
        }

        public static void SetTargetTexture(IPanel panel, RenderTexture targetTexture)
        {
            targetTextureField.SetValue(panel, targetTexture);
        }

        public static void DisposePanel(PanelSettings panelSettings)
        {
            disposePanel.Invoke(panelSettings, null);
        }
    }
}