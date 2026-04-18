using System;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// All types that should be accessible at runtime.
/// </summary>
public static class AllTypes
{
    public static readonly Type[] array;
    public static readonly Dictionary<ulong, Type> map = new();

    static AllTypes()
    {
        List<Type> list = new();
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
            Assembly assembly = assemblies[i];
            string assemblyName = assembly.GetName().Name;
            bool isProjectAssembly = assemblyName.StartsWith("Assembly-CSharp");
            bool isSystemAssembly = assemblyName.StartsWith("System") || assemblyName.StartsWith("mscorlib") ||
                assemblyName.StartsWith("UnityEngine") || assemblyName.StartsWith("UnityEditor") ||
                assemblyName.StartsWith("Unity.");
            if (isProjectAssembly && !isSystemAssembly)
            {
                Type[] types = assembly.GetTypes();
                for (int t = 0; t < types.Length; t++)
                {
                    Type type = types[t];
                    if (!type.IsAbstract && !type.IsInterface && type.IsPublic)
                    {
                        list.Add(type);
                        map[type.GetID()] = type;
                    }
                }
            }
        }

        array = list.ToArray();
    }
}
