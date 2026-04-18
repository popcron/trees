using System;
using System.Runtime.CompilerServices;

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
            Start();
#if UNITY_EDITOR
            AppDomain.CurrentDomain.DomainUnload += DomainUnloaded;
#else
            UnityEngine.Application.quitting += Shutdown;
#endif
        }

#if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
#endif
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
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
                Finish();
            }
        }

        private static void Start()
        {
            RuntimeHelpers.RunClassConstructor(typeof(UIEngine).TypeHandle);
        }

        private static void Finish()
        {
            UIEngine.CleanUp();
        }
    }
}