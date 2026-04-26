using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public static class UIEngine
    {
        public static readonly IPanel panel;
        public static readonly PanelSettings panelSettings;
        public static readonly ThemeStyleSheet themeStyleSheet;
        public static readonly Dictionary<UIBehaviour, VisualElement> map = new();
        public static readonly List<UIBehaviour> all = new();
        public static bool navigationEnabled = true;

        static UIEngine()
        {
            panelSettings = FindPanelSettings();
            themeStyleSheet = FindThemeStyleSheet();
            panel = InitializePanel();
        }

        public static void DisposePanel()
        {
            panelSettings.targetTexture = null;
            Reflection.DisposePanel(panelSettings);
        }

        private static IPanel InitializePanel()
        {
            Reflection.Create(panelSettings);
            IPanel panel = Reflection.GetPanel(panelSettings);
            Reflection.SetTargetTexture(panel, panelSettings.targetTexture);
            Reflection.SetSelectableGameObject(panel, null);
            Reflection.ApplyPanelSettings(panelSettings);
            panel.visualTree.styleSheets.Add(themeStyleSheet);
            panel.visualTree.RegisterCallback<NavigationMoveEvent>(OnNavigation, TrickleDown.TrickleDown);
            panel.visualTree.RegisterCallback<NavigationSubmitEvent>(OnNavigation, TrickleDown.TrickleDown);
            panel.visualTree.RegisterCallback<NavigationCancelEvent>(OnNavigation, TrickleDown.TrickleDown);
            return panel;
        }

        private static void OnNavigation<T>(T evt) where T : EventBase<T>, new()
        {
            if (navigationEnabled)
            {
                return;
            }

            evt.StopPropagation();
            panel.focusController.IgnoreEvent(evt);
        }

        public static PanelSettings FindPanelSettings()
        {
            PanelSettings panelSettings = Resources.Load<PanelSettings>("HUDPanelSettings");
            return panelSettings;
        }

        public static ThemeStyleSheet FindThemeStyleSheet()
        {
            ThemeStyleSheet themeStyleSheet = Resources.Load<ThemeStyleSheet>("HUDTheme");
            return themeStyleSheet;
        }

        public static Shader FindBlitShader()
        {
            Shader shader = Resources.Load<Shader>("HUDBlitShader");
            return shader;
        }
    }
}