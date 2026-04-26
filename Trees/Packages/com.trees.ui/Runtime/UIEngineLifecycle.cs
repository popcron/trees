using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;

namespace UI
{
    /// <summary>
    /// Makes sure that <see cref="UIEngine"/> has its
    /// start and stop called at the right time.
    /// </summary>
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
#endif
    public static class UIEngineLifecycle
    {
        private static bool shutdown;

        static UIEngineLifecycle()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            UnityEditor.EditorApplication.update += EditorUpdate;
            AppDomain.CurrentDomain.DomainUnload += DomainUnloaded;
#else
            Application.quitting += Shutdown;
#endif
            SetUp();
        }

#if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInitialize()
        {
        }

        private static void DomainUnloaded(object sender, EventArgs e)
        {
            Shutdown();
        }

        private static void Shutdown()
        {
            if (!shutdown)
            {
                shutdown = true;
                CleanUp();
            }
        }

        private static void SetUp()
        {
            RuntimeHelpers.RunClassConstructor(typeof(UIEngine).TypeHandle);
            RuntimeHelpers.RunClassConstructor(typeof(UIRendering).TypeHandle);
            RenderPipelineManager.endContextRendering += OnEndContextRendering;

            // inject Update into the player loop after PostLateUpdate
            PlayerLoopSystem playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            ref PlayerLoopSystem[] subSystems = ref playerLoop.subSystemList;
            for (int i = 0; i < subSystems.Length; i++)
            {
                ref PlayerLoopSystem subSystem = ref subSystems[i];
                if (subSystem.type == typeof(PostLateUpdate))
                {
                    Array.Resize(ref subSystem.subSystemList, subSystem.subSystemList.Length + 1);
                    ref PlayerLoopSystem newSubSystem = ref subSystem.subSystemList[^1];
                    newSubSystem.updateDelegate = Update;
                    newSubSystem.type = typeof(UIEngineLifecycle);
                }
            }

            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        private static void CleanUp()
        {
            RenderPipelineManager.endContextRendering -= OnEndContextRendering;
            UIRendering.ReleaseRenderTextures();
            UIEngine.DisposePanel();
        }

        private static void OnEndContextRendering(ScriptableRenderContext context, List<Camera> cameras)
        {
            UIRendering.OnEndContextRendering(context, cameras);
        }

        private static void Update()
        {
            IMUI.Update();
            IMUILayout.Update();
        }

#if UNITY_EDITOR
        private static void EditorUpdate()
        {
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                UIEngine.panel.visualTree.MarkDirtyRepaint();
                UnityEditor.SceneView.RepaintAll();
            }
        }

        private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                UIRendering.ReleaseRenderTextures();
            }
        }
#endif
    }
}