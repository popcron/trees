using Scripting;
using UnityEngine;

public class ColorTypeHandler : ITypeHandler<Color>
{
    Value ITypeHandler<Color>.Serialize(Color value)
    {
        if (value.a == 1f)
            return Value.Serialize($"#{ColorUtility.ToHtmlStringRGB(value)}");
        else
            return Value.Serialize($"#{ColorUtility.ToHtmlStringRGBA(value)}");
    }

    bool ITypeHandler<Color>.TryDeserialize(Value value, out Color result)
    {
        if (ColorUtility.TryParseHtmlString(value.ToString(), out result))
        {
            return true;
        }

        result = default;
        return false;
    }
}