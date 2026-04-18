using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

[InitializeOnLoad]
[DefaultExecutionOrder(int.MinValue)]
public static class ProgramLifecycle
{
    static ProgramLifecycle()
    {
        RuntimeHelpers.RunClassConstructor(typeof(Program).TypeHandle);
        EditorApplication.playModeStateChanged += PlayModeStateChanged;
    }

    [DidReloadScripts]
    private static void Initialize()
    {
    }

    private static void PlayModeStateChanged(PlayModeStateChange change)
    {
        if (change == PlayModeStateChange.EnteredEditMode)
        {
            ResetStaticFields();
        }
    }

    private static void ResetStaticFields()
    {
        FieldInfo[] fields = typeof(Program).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        foreach (FieldInfo field in fields)
        {
            if (field.FieldType.IsValueType)
            {
                field.SetValue(null, Activator.CreateInstance(field.FieldType));
            }
            else
            {
                field.SetValue(null, null);
            }
        }
    }
}
