using Scripting;
using UnityEngine;
using UnityEngine.InputSystem;

public static class Program
{
    public static Interpreter interpreter => ScriptingLibrary.interpreter;

    public static BaseBehaviour focalPoint;
    public static bool inControl;

    static Program()
    {
        RegisterBindings();
    }

    private static void RegisterBindings()
    {
        interpreter.DeclareBinding("time", () => Time.time);
        interpreter.DeclareBinding("focalPoint", () => focalPoint);
        interpreter.DeclareBinding("inControl", () => inControl);
        interpreter.DeclareBinding("hoveringOver", GetHoveringOver);
        interpreter.DeclareBinding("inEditMode", InEditMode);
    }

    public static BaseBehaviour GetHoveringOver()
    {
        if (Mouse.current is Mouse mouse)
        {
            Vector2 mousePosition = mouse.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            if (Physics.SphereCast(ray, 0.1f, out RaycastHit hit, 100f))
            {
                Unit unit = hit.collider.GetComponentInParent<Unit>();
                if (unit != null)
                {
                    return unit;
                }

                Prop prop = hit.collider.GetComponentInParent<Prop>();
                if (prop != null)
                {
                    return prop;
                }
            }
        }

        return null;
    }

    public static bool InEditMode()
    {
        return Application.isEditor && !Application.isPlaying;
    }
}
