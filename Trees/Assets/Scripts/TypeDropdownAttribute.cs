using System;
using UnityEngine;

public class TypeDropdownAttribute : PropertyAttribute
{
    public readonly Type deriveFrom;

    public TypeDropdownAttribute()
    {
        deriveFrom = null;
    }

    public TypeDropdownAttribute(Type deriveFrom)
    {
        this.deriveFrom = deriveFrom;
    }
}