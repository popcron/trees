using Scripting;
using UnityEngine;

public class Vector2TypeHandler : ObjectTypeHandler<Vector2>
{
    protected override void CreateTypeSymbol(TypeSymbol typeSymbol)
    {
        typeSymbol.fields.Add(new FieldSymbol(Value.Type.Float, "x"));
        typeSymbol.fields.Add(new FieldSymbol(Value.Type.Float, "y"));
    }

    protected override void Serialize(ObjectInstance instance, Vector2 value)
    {
        instance.fields[0] = new(value.x);
        instance.fields[1] = new(value.y);
    }

    protected override Vector2 Deserialize(ObjectInstance instance)
    {
        float x = (float)instance.fields[0].doubleValue;
        float y = (float)instance.fields[1].doubleValue;
        return new Vector2(x, y);
    }
}
