using Scripting;
using UnityEngine;

public class UnitTypeHandler : UnityObjectTypeHandler<Unit>
{
    public UnitTypeHandler()
    {
        type.fields.Add(new FieldSymbol(Value.Type.String, "color"));
    }

    protected override void Serialize(ObjectInstance baseValue, Unit unit)
    {
        baseValue.Set("color", unit.color);
        baseValue.DeclareReader("color", value =>
        {
            unit.color = value.Deserialize<Color>();
        });
    }
}
