using Scripting;
using System;
using UnityEngine;

public class ColorTypeHandler : ITypeHandler<Color>
{
    Value ITypeHandler<Color>.Serialize(Color value)
    {
        if (value.a >= 1f)
        {
            return Value.Serialize($"#{ColorUtility.ToHtmlStringRGB(value)}");
        }
        else
        {
            return Value.Serialize($"#{ColorUtility.ToHtmlStringRGBA(value)}");
        }
    }

    Color ITypeHandler<Color>.Deserialize(Value value)
    {
        if (!ColorUtility.TryParseHtmlString(value.Deserialize().ToString(), out Color result))
        {
            throw new InvalidOperationException($"Value of type {value.GetType()} cannot be deserialized as a Color");
        }

        return result;
    }
}