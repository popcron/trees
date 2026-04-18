using Scripting;
using UnityEngine;

public class TypeSelector : MonoBehaviour
{
    public string variableName;
    public SourceCode code;
    public bool misterBoolean;
    public SourceCode<int> dolorSitAmet;
    public float theFloater;
    public TypeID<BaseBehaviour> componentType;
    public byte the;
    public TypeID anyType;
    public Vector3 position;
    public SourceCode<Color> colorful = Color.red;

    private void Start()
    {
        Debug.Log(componentType);
        Debug.Log(anyType);
    }
}
