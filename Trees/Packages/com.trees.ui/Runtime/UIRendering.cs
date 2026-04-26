using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace UI
{
    public static class UIRendering
    {
        public static bool repaint;
        public static Vector2Int panelResolution;
        private static Material blitMaterial;
        private static readonly Dictionary<Camera, RenderTexture> renderTextures = new();

        public static void ReleaseRenderTextures()
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

        private static void SortByWorldZ()
        {
            UIEngine.all.Sort((a, b) => b.transform.position.z.CompareTo(a.transform.position.z));
            for (int i = 0; i < UIEngine.all.Count; i++)
            {
                UIEngine.map[UIEngine.all[i]].BringToFront();
            }
        }

        public static void OnEndContextRendering(ScriptableRenderContext context, List<Camera> cameras)
        {
            SortByWorldZ();
            if (repaint)
            {
                repaint = false;
                UIEngine.panel.visualTree.MarkDirtyRepaint();
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
            UIEngine.panelSettings.targetTexture = newRenderTexture;
            Reflection.SetTargetTexture(UIEngine.panel, newRenderTexture);
            Reflection.ApplyPanelSettings(UIEngine.panelSettings);

            // update all, i forget why
            for (int i = 0; i < UIEngine.all.Count; i++)
            {
                UIBehaviour component = UIEngine.all[i];
                VisualElement root = UIEngine.map[component];
                component.UpdateTransform(root);
            }

            UIEngine.panel.visualTree.MarkDirtyRepaint();
            created = true;
            return newRenderTexture;
        }

        private static void DrawToCamera(ScriptableRenderContext context, Camera camera, RenderTexture tex)
        {
            if (blitMaterial == null)
            {
                blitMaterial = new Material(UIEngine.FindBlitShader());
            }

            CommandBuffer cmd = CommandBufferPool.Get("UI" + camera.GetEntityId());
            cmd.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            cmd.SetViewport(new Rect(0, 0, camera.pixelWidth, camera.pixelHeight));
            cmd.Blit(tex, BuiltinRenderTextureType.CurrentActive, blitMaterial);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
            context.Submit();
        }
    }
}