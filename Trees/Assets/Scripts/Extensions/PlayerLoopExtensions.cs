using System;
using UnityEngine.LowLevel;

public static class PlayerLoopExtensions
{
    public static int AddCallback(this ref PlayerLoopSystem system, Action function, Type systemType = null)
    {
        int index = system.subSystemList.Length;
        PlayerLoopSystem[] subsystemList = new PlayerLoopSystem[index + 1];
        Array.Copy(system.subSystemList, subsystemList, system.subSystemList.Length);
        ref PlayerLoopSystem newCallbackSystem = ref subsystemList[index];
        newCallbackSystem.updateDelegate = new(function);
        newCallbackSystem.type = systemType;
        system.subSystemList = subsystemList;
        return index;
    }

    public static void RemoveCallback(this ref PlayerLoopSystem system, int index)
    {
        if (index < 0 || index >= system.subSystemList.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        PlayerLoopSystem[] subsystemList = new PlayerLoopSystem[system.subSystemList.Length - 1];
        if (index > 0)
        {
            Array.Copy(system.subSystemList, 0, subsystemList, 0, index);
        }

        if (index < system.subSystemList.Length - 1)
        {
            Array.Copy(system.subSystemList, index + 1, subsystemList, index, system.subSystemList.Length - index - 1);
        }

        system.subSystemList = subsystemList;
    }

    public static ref PlayerLoopSystem GetSubSystem<T>(this ref PlayerLoopSystem playerLoopSystem)
    {
        Type systemType = typeof(T);
        PlayerLoopSystem[] subSystems = playerLoopSystem.subSystemList;
        for (int i = 0; i < subSystems.Length; i++)
        {
            ref PlayerLoopSystem subSystem = ref subSystems[i];
            if (subSystem.type == systemType)
            {
                return ref subSystem;
            }
        }

        throw new InvalidOperationException($"Subsystem of type {typeof(T)} not found");
    }

    public static int IndexOf(this ref PlayerLoopSystem system, Type systemType)
    {
        for (int i = 0; i < system.subSystemList.Length; i++)
        {
            if (system.subSystemList[i].type == systemType)
            {
                return i;
            }
        }

        return -1;
    }
}