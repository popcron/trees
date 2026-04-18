using Scripting;
using UnityEngine;

public abstract class ObjectTypeHandler
{
    public static readonly TypeSymbol baseType;

    static ObjectTypeHandler()
    {
        FieldSymbol[] fields =
        {
            new(Value.Type.Long, "entityId"),
            new(Value.Type.String, "name")
        };

        baseType = new("Object", fields);
    }

    public readonly TypeSymbol type;

    public abstract System.Type ComponentType { get; }

    public ObjectTypeHandler()
    {
        type = baseType.Clone(ComponentType.Name);
    }
}

public abstract class ObjectTypeHandler<T> : ObjectTypeHandler, ITypeHandler<T> where T : Object
{
    public override System.Type ComponentType => typeof(T);

    public Value Serialize(T obj)
    {
        if (obj == null)
        {
            return Value.Null;
        }

        Value[] values = new Value[type.FieldCount];
        values[0] = Value.Serialize(obj.GetEntityId());
        values[1] = Value.Serialize(obj.name);
        ObjectInstance newInstance = new(type, values);
        Serialize(newInstance, obj);
        for (int i = 2; i < type.FieldCount; i++)
        {
            if (values[i] == default)
            {
                throw new System.InvalidOperationException($"Field '{type.fields[i].name}' was not set during serialization of {ComponentType.Name}.");
            }
        }

        return Value.Serialize(newInstance);
    }

    public bool TryDeserialize(Value value, out T obj)
    {
        if (value.IsNull)
        {
            obj = null;
            return true;
        }

        EntityId entityId = value.Object.values[0].Deserialize<EntityId>();
        obj = EntityIdRegistry.map[entityId] as T;
        return true;
    }

    protected virtual void Serialize(ObjectInstance baseValue, T obj)
    {
    }
}