using Scripting;
using UnityEngine;

public abstract class UnityObjectTypeHandler
{
    public static readonly TypeSymbol baseType;

    static UnityObjectTypeHandler()
    {
        FieldSymbol[] fields =
        {
            new(Value.Type.Integer, "entityId"),
            new(Value.Type.String, "name")
        };

        baseType = new("UnityObject", fields);
    }

    public readonly TypeSymbol type;

    public abstract System.Type ComponentType { get; }

    public UnityObjectTypeHandler()
    {
        type = baseType.Clone(ComponentType.Name);
    }
}

public abstract class UnityObjectTypeHandler<T> : UnityObjectTypeHandler, ITypeHandler<T> where T : Object
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
        ObjectInstance newInstance = new(type, values, null);
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

    public T Deserialize(Value value)
    {
        if (value == Value.Null)
        {
            return null;
        }

        EntityId entityId = value.objectValue.fields[0].Deserialize<EntityId>();
        return EntityIdRegistry.map[entityId] as T;
    }

    protected virtual void Serialize(ObjectInstance baseValue, T obj)
    {
    }
}