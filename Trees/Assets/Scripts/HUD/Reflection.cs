using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public static class Reflection
    {
        // a moment in silence for this shit stain piece of text, all because of unitys defensive programming
        // todo: find a neater workaround for uitk

        private const BindingFlags Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly Type panelSettingsType = typeof(PanelSettings);
        private static readonly Assembly uiElementsAssembly = panelSettingsType.Assembly;
        private static readonly Type runtimePanelType = uiElementsAssembly.GetType("UnityEngine.UIElements.RuntimePanel");
        private static readonly MethodInfo createMethod = runtimePanelType.GetMethod("Create", Static);
        private static readonly PropertyInfo panelProperty = panelSettingsType.GetProperty("panel", Instance);
        private static readonly Type baseRuntimePanelType = runtimePanelType.BaseType;
        private static readonly FieldInfo targetTextureField = baseRuntimePanelType.GetField("targetTexture", Instance);
        private static readonly PropertyInfo selectableGameObjectProperty = baseRuntimePanelType.GetProperty("selectableGameObject", Instance);
        private static readonly MethodInfo applyPanelSettings = panelSettingsType.GetMethod("ApplyPanelSettings", Instance);
        private static readonly MethodInfo disposePanel = panelSettingsType.GetMethod("DisposePanel", Instance);

        public static void Create(PanelSettings panelSettings)
        {
            createMethod.Invoke(null, new object[] { panelSettings });
        }

        public static IPanel GetPanel(PanelSettings panelSettings)
        {
            return (IPanel)panelProperty.GetValue(panelSettings);
        }

        public static RenderTexture GetTargetTexture(IPanel panel)
        {
            return (RenderTexture)targetTextureField.GetValue(panel);
        }

        public static void SetTargetTexture(IPanel panel, RenderTexture targetTexture)
        {
            targetTextureField.SetValue(panel, targetTexture);
        }

        public static GameObject GetSelectableGameObject(IPanel panel)
        {
            return (GameObject)selectableGameObjectProperty.GetValue(panel);
        }

        public static void SetSelectableGameObject(IPanel panel, GameObject selectableGameObject)
        {
            selectableGameObjectProperty.SetValue(panel, selectableGameObject);
        }

        public static void ApplyPanelSettings(PanelSettings panelSettings)
        {
            applyPanelSettings.Invoke(panelSettings, null);
        }

        public static void DisposePanel(PanelSettings panelSettings)
        {
            disposePanel.Invoke(panelSettings, null);
        }
    }
}