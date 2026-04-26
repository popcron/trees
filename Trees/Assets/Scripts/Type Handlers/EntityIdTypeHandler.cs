using Scripting;
using UnityEngine;

public class EntityIdTypeHandler : ITypeHandler<EntityId>
{
    Value ITypeHandler<EntityId>.Serialize(EntityId value)
    {
        return Value.Serialize(EntityId.ToULong(value));
    }

    EntityId ITypeHandler<EntityId>.Deserialize(Value value)
    {
        return EntityId.FromULong(value.Deserialize<ulong>());
    }
}
