using System;
using System.Collections.Generic;
using UnityEditor;

[InitializeOnLoad]
public static class EditorTypeTable
{
    public static readonly Dictionary<Type, Type[]> options = new();

    static EditorTypeTable()
    {
    }

    public static Type[] GetTypesDerivedFrom(Type deriveFrom)
    {
        if (!options.TryGetValue(deriveFrom, out Type[] optionsArray))
        {
            List<Type> list = new();
            foreach (Type type in TypeCache.GetTypesDerivedFrom(deriveFrom))
            {
                if (!type.IsAbstract)
                {
                    list.Add(type);
                }
            }

            optionsArray = list.ToArray();
            options.Add(deriveFrom, optionsArray);
        }

        return optionsArray;
    }
}