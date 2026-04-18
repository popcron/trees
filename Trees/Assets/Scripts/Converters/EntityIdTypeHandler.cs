using Scripting;
using UnityEngine;

public class EntityIdTypeHandler : ITypeHandler<EntityId>
{
    Value ITypeHandler<EntityId>.Serialize(EntityId value)
    {
        return Value.Serialize(EntityId.ToULong(value));
    }

    bool ITypeHandler<EntityId>.TryDeserialize(Value value, out EntityId result)
    {
        result = EntityId.FromULong(value.Deserialize<ulong>());
        return true;
    }
}
