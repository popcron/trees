using Scripting;
using UnityEngine;

public class TypeSelector : MonoBehaviour
{
    public string variableName;
    public SourceCode code;
    public bool misterBoolean;
    public SourceCode<int> dolorSitAmet;
    public float theFloater;

    [TypeDropdown(typeof(BaseBehaviour))]
    public ulong componentType;

    public byte the;

    [TypeDropdown]
    public ulong anyType;

    public Vector3 position;
    public SourceCode<Color> colorful = Color.red;

    private void Start()
    {
        Debug.Log(componentType);
        Debug.Log(anyType);
    }
}
