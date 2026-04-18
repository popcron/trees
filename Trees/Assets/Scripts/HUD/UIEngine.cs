using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace UI
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class UIEngine
    {
        public static bool repaint;
        public static Vector2Int panelResolution;
        public static float sceneView2DPositionScale = 1f;
        public static float sceneView2DPositionOffset = 0f;
        public static readonly IPanel thePanel;
        public static readonly PanelSettings panelSettings;
        public static readonly ThemeStyleSheet themeStyleSheet;
        public static readonly Dictionary<UIBehaviour, VisualElement> map = new();
        public static readonly List<UIBehaviour> all = new();
        private static Material blitMaterial;
        private static readonly Dictionary<Camera, RenderTexture> renderTextures = new();

        static UIEngine()
        {
            panelSettings = FindPanelSettings();
            themeStyleSheet = FindThemeStyleSheet();
            thePanel = InitializePanel();
            blitMaterial = new Material(FindBlitShader());
            RenderPipelineManager.endContextRendering += OnEndContextRendering;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            UnityEditor.EditorApplication.update += EditorUpdate;
#endif
        }

        public static void CleanUp()
        {
            RenderPipelineManager.endContextRendering -= OnEndContextRendering;
            ReleaseRenderTextures();
            DisposePanel();
        }

        private static void DisposePanel()
        {
            panelSettings.targetTexture = null;
            Reflection.DisposePanel(panelSettings);
        }

        private static void ReleaseRenderTextures()
        {
            foreach (RenderTexture tex in renderTextures.Values)
            {
                if (tex != null)
                {
                    tex.Release();
                }
            }

            renderTextures.Clear();
        }

        private static IPanel InitializePanel()
        {
            Reflection.Create(panelSettings);
            IPanel panel = Reflection.GetPanel(panelSettings);
            Reflection.SetTargetTexture(panel, panelSettings.targetTexture);
            Reflection.SetSelectableGameObject(panel, null);
            Reflection.ApplyPanelSettings(panelSettings);
            panel.visualTree.styleSheets.Add(themeStyleSheet);
            return panel;
        }

        private static void SortByWorldZ()
        {
            all.Sort((a, b) => b.transform.position.z.CompareTo(a.transform.position.z));
            for (int i = 0; i < all.Count; i++)
            {
                map[all[i]].BringToFront();
            }
        }

        private static void OnEndContextRendering(ScriptableRenderContext context, List<Camera> cameras)
        {
            SortByWorldZ();
            if (repaint)
            {
                repaint = false;
                thePanel.visualTree.MarkDirtyRepaint();
            }

            foreach (Camera camera in cameras)
            {
                panelResolution.x = (int)camera.pixelRect.width;
                panelResolution.y = (int)camera.pixelRect.height;
                if (camera.cameraType == CameraType.Game)
                {
                    RenderTexture tex = GetOrCreateRenderTexture(camera, out _);
                    DrawToCamera(context, camera, tex);
                }
#if UNITY_EDITOR
                else if (camera.cameraType == CameraType.SceneView)
                {
                    break; // todo: cant get the scene view rendered size to match
                    UnityEditor.SceneView sceneView = GetSceneView(camera);
                    if (sceneView != null)
                    {
                        RenderTexture tex = GetOrCreateRenderTexture(camera, out bool created);
                        if (created)
                        {
                            UnityEditor.EditorApplication.delayCall += UnityEditor.SceneView.RepaintAll;
                        }

                        DrawToCamera(context, camera, tex);
                    }
                }
#endif
            }
        }

        private static RenderTexture GetOrCreateRenderTexture(Camera camera, out bool created)
        {
            int camWidth = (int)camera.pixelRect.width;
            int camHeight = (int)camera.pixelRect.height;
            if (renderTextures.TryGetValue(camera, out RenderTexture existing) && existing != null)
            {
                if (existing.width == camWidth && existing.height == camHeight)
                {
                    created = false;
                    return existing;
                }

                existing.Release();
            }

            RenderTextureDescriptor desc = new(camWidth, camHeight, RenderTextureFormat.ARGB32, 0);
            RenderTexture newRenderTexture = new(desc);
            newRenderTexture.Create();
            renderTextures[camera] = newRenderTexture;
            panelSettings.targetTexture = newRenderTexture;
            Reflection.SetTargetTexture(thePanel, newRenderTexture);
            Reflection.ApplyPanelSettings(panelSettings);

            // update all, forget why
            for (int i = 0; i < all.Count; i++)
            {
                UIBehaviour component = all[i];
                VisualElement root = map[component];
                component.UpdateTransform(root);
            }

            thePanel.visualTree.MarkDirtyRepaint();
            created = true;
            return newRenderTexture;
        }

        private static void DrawToCamera(ScriptableRenderContext context, Camera camera, RenderTexture tex)
        {
            if (blitMaterial == null)
            {
                blitMaterial = new Material(FindBlitShader());
            }

            CommandBuffer cmd = CommandBufferPool.Get("HUD_Render_Pass" + camera.GetEntityId());
            cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            cmd.SetViewport(new Rect(0, 0, camera.pixelWidth, camera.pixelHeight));
            cmd.Blit(tex, BuiltinRenderTextureType.CurrentActive, blitMaterial);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
            context.Submit();
        }

#if UNITY_EDITOR
        private static UnityEditor.SceneView GetSceneView(Camera camera)
        {
            foreach (UnityEditor.SceneView sceneView in UnityEditor.SceneView.sceneViews)
            {
                if (sceneView.camera == camera)
                {
                    return sceneView;
                }
            }

            return null;
        }
#endif

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

#if UNITY_EDITOR
        private static void EditorUpdate()
        {
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                thePanel.visualTree.MarkDirtyRepaint();
                UnityEditor.SceneView.RepaintAll();
            }
        }

        private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                ReleaseRenderTextures();
            }
        }
#endif
    }
}