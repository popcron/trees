using System.Runtime.CompilerServices;
using UnityEngine;

public static class EntityIdExtensions
{
    public static ulong GetLongHashCode(this EntityId entityId)
    {
        return Unsafe.As<EntityId, ulong>(ref entityId);
    }
}